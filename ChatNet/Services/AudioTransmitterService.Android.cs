using Android.App;
using Android.Content;
using Android.Media;
using Android.Media.Projection;
using Android.OS;
using Android.Runtime;
using Android.Util;
using AndroidX.Core.App;
using Sockets.Plugin;
using Java.IO;
using Java.Lang;
using Java.Util;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

namespace ChatNet.Services
{
    [Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeMediaProjection)]
    public partial class AudioTransmitterService : Service
    {
        private const string LOG_TAG = "AudioTransmitterService";
        private const int SERVICE_ID = 123;
        private const string NOTIFICATION_CHANNEL_ID = "AudioTransmitter channel";

        private const int NUM_SAMPLES_PER_READ = 1024;
        private const int BYTES_PER_SAMPLE = 2; // 2 bytes since we hardcoded the PCM 16-bit format
        private const int BUFFER_SIZE_IN_BYTES = NUM_SAMPLES_PER_READ * BYTES_PER_SAMPLE;

        public const string ACTION_START = "AudioTransmitterService:Start";
        public const string ACTION_STOP = "AudioTransmitterService:Stop";
        public const string EXTRA_RESULT_DATA = "AudioTransmitterService:Extra:ResultData";

        private MediaProjectionManager mediaProjectionManager = null!;
        private MediaProjection? mediaProjection = null;

        private Java.Lang.Thread acceptConnectionsThread = null!;
        private Java.Lang.Thread audioCaptureThread = null!;

        private AudioRecord? audioRecord = null;

        public override void OnCreate()
        {
            base.OnCreate();
            CreateNotificationChannel();
            StartForeground(SERVICE_ID, new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID).Build());
            mediaProjectionManager = ApplicationContext.GetSystemService(MediaProjectionService).JavaCast<MediaProjectionManager>();
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var serviceChannel = new NotificationChannel
            (
                NOTIFICATION_CHANNEL_ID,
                "Audio Transmitter Service Channel",
                NotificationImportance.Default
            );
            var manager = ApplicationContext.GetSystemService(NotificationService).JavaCast<NotificationManager>();
            manager.CreateNotificationChannel(serviceChannel);
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent != null)
            {
                switch (intent.Action)
                {
                    case ACTION_START:
                        System.Console.WriteLine($"STARTEDDDD Transmissionnnnnnnnn");
                        mediaProjection = mediaProjectionManager.GetMediaProjection((int)Result.Ok, intent.GetParcelableExtra(EXTRA_RESULT_DATA).JavaCast<Intent>()!);
                        StartAudioTransmission();
                        return StartCommandResult.Sticky;

                    case ACTION_STOP:
                        System.Console.WriteLine($"STOPPEDDDDD Transmissionnnnnnnnn");
                        StopAudioTransmission();
                        return StartCommandResult.NotSticky;

                    default:
                        throw new IllegalArgumentException($"Unexpected action received: {intent.Action}");
                }
            }
            else
            {
                return StartCommandResult.NotSticky;
            }

            return base.OnStartCommand(intent, flags, startId);
        }

        public partial async Task StartAudioTransmission()
        {
            string multicastAddress = "239.255.0.1";
            int multicastPort = 6969;

            try
            {
                server = new MulticastServer(IPAddress.Any, 0);
                server.OptionReuseAddress = true;
                server.Start(multicastAddress, multicastPort);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }

            //acceptConnectionsThread = new Java.Lang.Thread(async () =>
            //{
            //    while (!acceptConnectionsThread.IsInterrupted)
            //    {
            //        if (_client.Server.Pending())
            //        {
            //            System.Console.Write("Waiting for a connection... ");
            //            TcpClient newClient = await _client.Server.AcceptTcpClientAsync();
            //            if (_client.ConnectedClients.TryAdd(_client.ConnectedClients.Count + 1, newClient))
            //            {
            //                System.Console.WriteLine("Connected!!!!!!!!");
            //                Log.Debug(LOG_TAG, $"Connected!!!!!!!!");
            //            }
            //            //network = _tcpClient.GetStream();
            //        }
            //    }
            //});
            //acceptConnectionsThread.Start();

            var config = new AudioPlaybackCaptureConfiguration.Builder(mediaProjection!)
                .AddMatchingUsage(AudioUsageKind.Media) // TODO provide UI options for inclusion/exclusion
                .Build();

            var audioFormat = new AudioFormat.Builder()
                .SetEncoding(Encoding.Pcm16bit)
                .SetSampleRate(48000)
                .SetChannelMask(ChannelOut.Stereo)
                .Build();

            audioRecord = new AudioRecord.Builder()
                .SetAudioFormat(audioFormat)
                // For optimal performance, the buffer size
                // can be optionally specified to store audio samples.
                // If the value is not specified,
                // uses a single frame and lets the
                // native code figure out the minimum buffer size.
                .SetBufferSizeInBytes(BUFFER_SIZE_IN_BYTES)
                .SetAudioPlaybackCaptureConfig(config)
                .Build();

            audioRecord!.StartRecording();

            audioCaptureThread = new Java.Lang.Thread(async () =>
            {
                //var outputFile = CreateAudioFile();
                //Log.Debug(LOG_TAG, $"Created file for capture target: {outputFile.AbsolutePath}");
                await TransmitAudioToDevices();
            });
            audioCaptureThread.Start();
        }

        //private async void EstablishConnectionsAsync()
        //{
        //    foreach (var device in BluetoothAdapter.DefaultAdapter.BondedDevices)
        //    {
        //        System.Console.WriteLine(device.Name);
        //    }
        //}

        //private File CreateAudioFile()
        //{
        //    var audioCapturesDirectory = new File(GetExternalFilesDir(null), "/AudioCaptures");
        //    if (!audioCapturesDirectory.Exists())
        //    {
        //        audioCapturesDirectory.Mkdirs();
        //    }
        //    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //    var fileName = $"Capture-{timestamp}.pcm";
        //    var createdFile = new File(audioCapturesDirectory.AbsolutePath + "/" + fileName);
        //    createdFile.CreateNewFile();
        //    return createdFile;
        //}

        private async Task TransmitAudioToDevices()
        {
            var capturedAudioSamples = new byte[NUM_SAMPLES_PER_READ];

            while (!audioCaptureThread.IsInterrupted)
            {
                await audioRecord?.ReadAsync(capturedAudioSamples, 0, NUM_SAMPLES_PER_READ);
                server.Multicast(capturedAudioSamples);

                // This loop should be as fast as possible to avoid artifacts in the captured audio
                // You can uncomment the following line to see the capture samples but
                // that will incur a performance hit due to logging I/O.
                // Log.v(LOG_TAG, "Audio samples captured: ${capturedAudioSamples.toList()}")

                //await network.WriteAsync(capturedAudioSamples, 0, NUM_SAMPLES_PER_READ);

                //foreach (var client in _client.ConnectedCl)
                //{
                //    try
                //    {
                //        NetworkStream nws = client.Value.GetStream();

                //        if (!nws.Socket.Connected)
                //        {
                //            if (_client.ConnectedClients.TryRemove(client.Key, out TcpClient tcpClient))
                //            {
                //                System.Console.WriteLine($"Client ['{client.Key}'] has been disconnectedddddd!!!!!!!!");
                //                Log.Debug(LOG_TAG, $"Client ['{client.Key}'] has been disconnectedddddd!!!!!!!!");
                //            }
                //        }

                //        await client.Value.Client.SendAsync(capturedAudioSamples.AsMemory(0, NUM_SAMPLES_PER_READ), SocketFlags.None);
                //        Log.Debug(LOG_TAG, $"Sent {capturedAudioSamples.Length} bytes to client ['{client.Key}'] !!!!!!!!");
                //    }
                //    catch (System.Exception e)
                //    {
                //        System.Console.WriteLine(e.ToString());

                //        throw;
                //    }
                //}

                //fileOutputStream.Write(capturedAudioSamples, 0, BUFFER_SIZE_IN_BYTES / 2);
            }

            //fileOutputStream.Close();
            //Log.Debug(LOG_TAG, $"Audio capture finished for {outputFile.AbsolutePath}. File size is {outputFile.Length()} bytes.");
        }

        public partial void StopAudioTransmission()
        {
            Objects.RequireNonNull(mediaProjection, "Tried to stop audio transmission, but there was no ongoing transmission in place!");

            audioCaptureThread.Interrupt();
            audioCaptureThread.Join();

            audioRecord!.Stop();
            audioRecord!.Release();
            audioRecord = null;

            mediaProjection!.Stop();
            server.Dispose();
            StopSelf();
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        private void OnReceiveMessage(Message msg)
        {
            System.Console.WriteLine($"- [BROADCASTER] {msg.Text}");
        }
    }
}
