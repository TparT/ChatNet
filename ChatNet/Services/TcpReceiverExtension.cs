using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatNet.Services
{
    internal static class TcpReceiverExtension
    {
        static byte[] buffer;
        public static async Task StartReceivingAsync(this Socket client)
        {
            //client.BeginReceive(buffer)
        }
    }
}
