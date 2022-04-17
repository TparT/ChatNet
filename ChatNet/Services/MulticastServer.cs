using NetCoreServer;
using System.Net;
using System.Net.Sockets;

namespace ChatNet.Services
{
    public class MulticastServer : UdpServer
    {
        public MulticastServer(IPAddress address, int port) : base(address, port) { }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Multicast UDP server caught an error with code {error}");
        }
    }
}
