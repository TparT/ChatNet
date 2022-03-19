using Android.Media;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Android.Content;

namespace ChatNet.Services
{
    public partial class AudioPlayerManager
    {
        AudioTrack audioTrack;
        bool paused = false;
        CancellationTokenSource Cancellation = new CancellationTokenSource();
        int offset = 0;
        MemoryStream ms = new MemoryStream();

        public partial async Task Initialize()
        {
            int size = AudioTrack.GetMinBufferSize(48000, ChannelOut.Stereo, Android.Media.Encoding.Pcm16bit);
            var track = new AudioTrack.Builder();

            track.SetAudioAttributes(
                new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Media)
                .SetLegacyStreamType(Android.Media.Stream.Music)
                .SetContentType(AudioContentType.Music)
                //.SetFlags(AudioFlags.LowLatency)
                .Build());

            track.SetTransferMode(AudioTrackMode.Stream);
            //track.SetPerformanceMode(AudioTrackPerformanceMode.LowLatency);
            track.SetBufferSizeInBytes(size);

            track.SetAudioFormat(
                new AudioFormat.Builder()
                .SetChannelMask(ChannelOut.Stereo)
                .SetEncoding(Android.Media.Encoding.Pcm16bit)
                .SetSampleRate(48000).Build());

            audioTrack = track.Build();

            audioTrack.Play();
        }


        public partial async Task LoadAudioAsync(System.IO.Stream stream)
        {

            //player.Loa

            //AudioManager audioMan = (AudioManager)Android.App.Application.Context.GetSystemService(Context.AudioService);
            stream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            byte[] buff = ms.GetBuffer();

            try
            {



                //audioTrack = new AudioTrack(Android.Media.Stream.Music, 48000, ChannelOut.Mono, Android.Media.Encoding.Mp3, size, AudioTrackMode.Stream);

                //audioTrack = new AudioTrack
                //(
                //    AudioAttributes.Creator,
                //    new AudioFormat.Builder().Build().
                //);
            }
            catch (Exception ex) { }







        }

        public partial async Task WriteAsync(byte[] data, int offset, int count)
        {
            await audioTrack.WriteAsync(data, offset, count);
        }

        public partial async Task Play()
        {
            paused = false;
            byte[] data = new byte[1024];
            int count;

            try
            {
                audioTrack.Play();


                while ((count = await ms.ReadAsync(data, 0, data.Length)) != 0 && !Cancellation.IsCancellationRequested)
                {
                    if (Cancellation.IsCancellationRequested)
                        break;

                    await audioTrack.WriteAsync(data, offset, count);
                    //Array.Clear(data);
                    offset += count;
                }

            }
            catch (Exception ex)
            {

            }
        }

        public partial void Pause()
        {
            paused = true;
            Cancellation.Cancel();
        }
    }
}
