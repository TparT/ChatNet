using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatNet.Services
{
    public class ServerConnectionClient : IAsyncDisposable
    {
        public ConcurrentDictionary<int, TcpClient> ConnectedClients;
        public TcpListener Server;
        private bool _initialized = false;

        public event Action<string> DataReceived;

        public async Task Initialize()
        {
            try
            {
                if (!_initialized)
                {
                    ConnectedClients = new ConcurrentDictionary<int, TcpClient>();
                    Server = new TcpListener(IPAddress.Any, 6969);
                    Server.Start();
                    Console.WriteLine("Server has started on {0}.", Server.LocalEndpoint);
                    _initialized = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void BeginReceiving()
        {

        }

        public void Data(string data, byte[] buffer)
            => DataReceived?.Invoke(data);

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
