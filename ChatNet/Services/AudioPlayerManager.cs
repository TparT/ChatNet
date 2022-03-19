using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatNet.Services
{
    public partial class AudioPlayerManager
    {
        public partial Task Initialize();

        public partial Task WriteAsync(byte[] data, int offset, int count);

        public partial Task LoadAudioAsync(Stream stream);

        public partial Task Play();
        public partial void Pause();
    }
}
