using Sockets.Plugin;
using Sockets.Plugin.Abstractions;
using System.Collections.Concurrent;

namespace ChatNet.Services
{
    public partial class AudioTransmitterService
    {
        readonly ConcurrentDictionary<int, ITcpSocketClient> _clients = new ConcurrentDictionary<int, ITcpSocketClient>();
        MulticastServer server;

        private CancellationTokenSource _canceller;

        public partial Task StartAudioTransmission();
        public partial void StopAudioTransmission();
    }
}
