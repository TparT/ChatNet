using NetCoreServer;
using System.Net;
using System.Text;

namespace ChatNet.Services
{
    public class MulticastPlayerClient : UdpClient
    {
        public string Multicast;
        private AudioPlayerManager _player { get; set; } = default!;

        public MulticastPlayerClient(string address, int port, AudioPlayerManager player) : base(address, port) { _player = player; }

        public void DisconnectAndStop()
        {
            _stop = true;
            Disconnect();
            while (IsConnected)
                Thread.Yield();
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"Multicast UDP client connected a new session with Id {Id}");

            // Join UDP multicast group
            JoinMulticastGroup(Multicast);

            // Start receive datagrams
            ReceiveAsync();
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Multicast UDP client disconnected a session with Id {Id}");

            // Wait for a while...
            Thread.Sleep(1000);

            // Try to connect again
            if (!_stop)
                Connect();
        }

        protected override async void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            //string data = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            //Console.WriteLine("Incoming: " + data);
            await _player.WriteAsync(buffer, (int)offset, (int)size);
            //foreach (var sub in OnReceivedEvent.GetInvocationList())
            //{
            //    sub.DynamicInvoke(this, data);
            //}

            // Continue receive datagrams
            ReceiveAsync();
        }

        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            Console.WriteLine($"Multicast UDP client caught an error with code {error}");
        }

        private bool _stop;
    }
}
