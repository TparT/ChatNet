using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatNet.Services
{
    public partial class AudioTransmitterService
    {
        private ServerConnectionClient _client;
        private TcpClient _tcpClient;
        NetworkStream network = null;

        public partial Task StartAudioTransmission();
        public partial void StopAudioTransmission();
    }
}
