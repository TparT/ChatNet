using Sockets.Plugin.Abstractions;
using System.Text;

public class Message
{
    public string Text { get; set; }
    public DateTime Time { get; set; }
}

public static class SocketExtensions
{
    /// <summary>
    /// Continuously read strings from the socketclient and yield them as <see cref="Message"/> instances.
    /// <br />
    /// <br />
    /// Messages are delimited by <paramref name="eom"/>.
    /// <br />
    /// Stops when <paramref name="eof"/> is hit.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="eom"></param>
    /// <param name="eof"></param>
    /// <returns></returns>
    public static IEnumerable<Message> ReadStrings(this ITcpSocketClient client, CancellationToken cancellationToken, string eom = "<EOM>", string eof = "<EOF>")
    {
        //var from = String.Format("{0}:{1}", client.RemoteAddress, client.RemotePort);

        var currData = "";
        int bytesRec = 0;
        bool gotEof = false;

        while (bytesRec != -1 && !cancellationToken.IsCancellationRequested && !gotEof)
        {
            var buffer = new byte[1024];
            bytesRec = client.ReadStream.Read(buffer, 0, buffer.Length);
            currData += Encoding.UTF8.GetString(buffer, 0, bytesRec);

            // Hit an EOM - we have a full message in currData;
            if (currData.IndexOf(eom, StringComparison.Ordinal) > -1)
            {
                var msg = new Message
                {
                    Text = currData.Substring(0, currData.IndexOf(eom)),
                    Time = DateTime.Now
                    //DetailText = String.Format("<Received from {0} at {1}>", from, DateTime.Now.ToString("HH:mm:ss"))
                };

                yield return msg;

                currData = currData.Substring(currData.IndexOf(eom) + eom.Length);
            }

            // Hit an EOF - client is gracefully disconnecting
            if (currData.IndexOf(eof, StringComparison.Ordinal) > -1)
            {
                var msg = new Message
                {
                    Text = $"<{client.RemoteAddress}:{client.RemotePort} disconnected at {DateTime.Now:HH:mm:ss}>",
                    Time = DateTime.Now
                    //DetailText = String.Format("<{0} disconnected at {1}>", from, DateTime.Now.ToString("HH:mm:ss"))
                };

                yield return msg;

                gotEof = true;
            }
        }

        // if we get here, either the stream broke, the cancellation token was cancelled, or the eof message was received
        // time to drop the client.

        try
        {
            client.DisconnectAsync().Wait();
            client.Dispose();
        }
        catch { }
    }

    /// <summary>
    /// Writes a string to a socket client stream in UT8 with the specified <paramref name="eom"/> identifier.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="s"></param>
    /// <param name="eom"></param>
    /// <returns></returns>
    public async static Task WriteStringAsync(this ITcpSocketClient client, string s, string eom = "<EOM>")
    {
        var bytes = (s + eom).ToUTF8Bytes();

        await client.WriteStream.WriteAsync(bytes, 0, bytes.Length);
        await client.WriteStream.FlushAsync();
    }

    public static byte[] ToUTF8Bytes(this string s)
    {
        return Encoding.UTF8.GetBytes(s);
    }

    public static string ToStringFromUTF8Bytes(this byte[] buf)
    {
        return Encoding.UTF8.GetString(buf, 0, buf.Length);
    }

};