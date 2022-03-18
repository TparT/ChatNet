using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatNet.Services
{
    public class ServerConnectionClient
    {
        public TcpListener Server;
        private bool _initialized = false;

        public event Action<string> DataReceived;

        public async Task Initialize()
        {
            try
            {
                if (!_initialized)
                {
                    Server = new TcpListener(IPAddress.Parse("10.0.0.16"), 6969);
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
    }
}
