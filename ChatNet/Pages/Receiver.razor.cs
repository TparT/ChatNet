using ChatNet.Services;
using Microsoft.AspNetCore.Components;
using System.Net;

namespace ChatNet.Pages
{
    public partial class Receiver : IDisposable
    {
        //[Inject]
        //private ServerConnectionClient _client { get; set; } = default!;

        private string IpAddress { get; set; } = "10.0.0.16";
        int count = 0;

        private MulticastPlayerClient _client;

        [Inject]
        private AudioPlayerManager _player { get; set; } = default!;

        [Inject]
        private NetworkingService _networking { get; set; } = default!;

        private CancellationTokenSource _canceller;
        //TcpClient client;
        //NetworkStream stream;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await _player.Initialize();

                string listenAddress = IPAddress.Any.ToString();
                string multicastAddress = "239.255.0.1";
                int multicastPort = 6969;

                _networking.EnableMulticasting();
                _client = new MulticastPlayerClient(listenAddress, multicastPort, _player);
                _client.SetupMulticast(true);
                _client.Multicast = multicastAddress;
            }
        }

        private void Connect()
        {
            _client.Connect();

            //BeginReceive();
        }

        private void Reconnect()
        {
            _client.Disconnect();
        }

        private void Disconnect()
        {
            _client.DisconnectAndStop();
            _networking.DisableMulticasting();
        }

        //private async void BeginReceive()
        //{
        //    Task.Factory.StartNew(async () =>
        //    {
        //        foreach (var msg in _client.ReadStrings(_canceller.Token))
        //        {
        //            byte[] bytes = msg.Text.ToUTF8Bytes();
        //            await _player.WriteAsync(bytes, 0, bytes.Length);
        //        }
        //    }, TaskCreationOptions.LongRunning);
        //}


        //private async void ReceiveCallback(IAsyncResult ar)
        //{
        //    try
        //    {
        //        // Retrieve the state object and the client socket
        //        // from the asynchronous state object.  
        //        StateObject state = (StateObject)ar.AsyncState;
        //        Socket client = state.workSocket;

        //        // Read data from the remote device.  
        //        int bytesRead = client.EndReceive(ar);

        //        if (bytesRead > 0)
        //        {
        //            await Console.Out.WriteLineAsync($"Received [{state.buffer.Length}] bytes from server! ({count++})");

        //            // There might be more data, so store the data received so far.  
        //            state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

        //            // Get the rest of the data.  
        //            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);

        //            //DataPacketReceived?.Invoke(state.buffer);
        //        }
        //        else
        //        {
        //            //FullPacketReceived?.Invoke(state.sb.ToString());
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("[SERVER] INITIALIZED SERVER PAGE!");



            //string address = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "127.0.0.1";

            //Buffer for reading data
            //byte[] bytes = new byte[1024];
            //string data = null;

            //await Task.Factory.StartNew(async () =>
            //{
            //    Task.Delay(500);
            //    StateHasChanged();
            //});

            //await Task.Factory.StartNew(async () =>
            //{
            //    // Enter the listening loop.
            //    //await _client.Initialize();



            //    while (true)
            //    {

            //        data = null;

            //        // Perform a blocking call to accept requests.
            //        // You could also use server.AcceptSocket() here.


            //        // Get a stream object for reading and writing
            //        stream = client.GetStream();

            //        int i;

            //        // Loop to receive all the data sent by the client.
            //        while ((i = stream.Read(bytes, 0, bytes.Length)) != -1)
            //        {
            //            await _player.WriteAsync(bytes, 0, i);

            //            // Translate data bytes to a ASCII string.
            //            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
            //            Console.WriteLine("Received: {0}", data);
            //            //_ += Environment.NewLine + data;
            //            //DataReceived?.Invoke(data);
            //            //// Process the data sent by the client.
            //            //data = data.ToUpper();

            //            //byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

            //            //// Send back a response.
            //            //stream.Write(msg, 0, msg.Length);
            //            //Console.WriteLine("Sent: {0}", data);
            //        }

            //        // Shutdown and end connection
            //        client.Close();
            //    }
            //});
        }

        public void Dispose()
        {
            _client.DisconnectAndStop();
            _client.Dispose();
            _networking.DisableMulticasting();
        }
    }
}
