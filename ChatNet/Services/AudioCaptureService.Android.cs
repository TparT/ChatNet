using Android.App;
using Android.Content;
using Android.Media;
using Android.Media.Projection;
using Android.OS;
using Android.Runtime;
using Android.Util;
using AndroidX.Core.App;
using Java.IO;
using Java.Lang;
using Java.Util;
using File = Java.IO.File;

namespace ChatNet.Services
{
    //[Activity(Label = "AudioCaptureService START", Name = "com/example/MyIntentTest/AudioCaptureService")]
    //[IntentFilter(new[] { "com.TheTparT.ChatNet.AudioCaptureService:Start" })]
    //public class AudioCaptureStartIntentService : Activity
    //{
    //    protected override void OnCreate(Bundle savedInstanceState)
    //    {
    //        base.OnCreate(savedInstanceState);
    //        mediaProjection = mediaProjectionManager.GetMediaProjection((int)Result.Ok, null);
    //        StartAudioCapture();
    //    }
    //}

    [Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeMediaProjection)]
    public partial class AudioCaptureService : Service
    {
        private const string LOG_TAG = "AudioCaptureService";
        private const int SERVICE_ID = 123;
        private const string NOTIFICATION_CHANNEL_ID = "AudioCapture channel";

        private const int NUM_SAMPLES_PER_READ = 1024;
        private const int BYTES_PER_SAMPLE = 2; // 2 bytes since we hardcoded the PCM 16-bit format
        private const int BUFFER_SIZE_IN_BYTES = NUM_SAMPLES_PER_READ * BYTES_PER_SAMPLE;

        public const string ACTION_START = "AudioCaptureService:Start";
        public const string ACTION_STOP = "AudioCaptureService:Stop";
        public const string EXTRA_RESULT_DATA = "AudioCaptureService:Extra:ResultData";

        private MediaProjectionManager mediaProjectionManager = null!;
        private MediaProjection? mediaProjection = null;

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
                "Audio Capture Service Channel",
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
                        System.Console.WriteLine($"STARTEDDDD");
                        mediaProjection = mediaProjectionManager.GetMediaProjection((int)Result.Ok, intent.GetParcelableExtra(EXTRA_RESULT_DATA).JavaCast<Intent>()!);
                        StartAudioCapture();
                        return StartCommandResult.Sticky;

                    case ACTION_STOP:
                        System.Console.WriteLine($"STOPPEDDDDD");
                        StopAudioCapture();
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

        public partial void StartAudioCapture()
        {
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
            audioCaptureThread = new Java.Lang.Thread(new Runnable(new Action(delegate
            {
                var outputFile = CreateAudioFile();
                Log.Debug(LOG_TAG, $"Created file for capture target: {outputFile.AbsolutePath}");
                WriteAudioToFile(outputFile);
            })));
            audioCaptureThread.Start();
        }

        private File CreateAudioFile()
        {
            var audioCapturesDirectory = new File(GetExternalFilesDir(null), "/AudioCaptures");
            if (!audioCapturesDirectory.Exists())
            {
                audioCapturesDirectory.Mkdirs();
            }
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var fileName = $"Capture-{timestamp}.pcm";
            var createdFile = new File(audioCapturesDirectory.AbsolutePath + "/" + fileName);
            createdFile.CreateNewFile();
            return createdFile;
        }

        private void WriteAudioToFile(File outputFile)
        {
            var fileOutputStream = new FileOutputStream(outputFile);
            var capturedAudioSamples = new byte[NUM_SAMPLES_PER_READ];

            while (!audioCaptureThread.IsInterrupted)
            {
                audioRecord?.Read(capturedAudioSamples, 0, NUM_SAMPLES_PER_READ);

                // This loop should be as fast as possible to avoid artifacts in the captured audio
                // You can uncomment the following line to see the capture samples but
                // that will incur a performance hit due to logging I/O.
                // Log.v(LOG_TAG, "Audio samples captured: ${capturedAudioSamples.toList()}")

                fileOutputStream.Write(capturedAudioSamples, 0, BUFFER_SIZE_IN_BYTES / 2);
            }

            fileOutputStream.Close();
            Log.Debug(LOG_TAG, $"Audio capture finished for {outputFile.AbsolutePath}. File size is {outputFile.Length()} bytes.");
        }

        public partial void StopAudioCapture()
        {
            Objects.RequireNonNull(mediaProjection, "Tried to stop audio capture, but there was no ongoing capture in place!");

            audioCaptureThread.Interrupt();
            audioCaptureThread.Join();

            audioRecord!.Stop();
            audioRecord!.Release();
            audioRecord = null;

            mediaProjection!.Stop();
            StopSelf();
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }
}
