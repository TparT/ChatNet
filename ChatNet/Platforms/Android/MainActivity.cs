using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media.Projection;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using ChatNet.Services;

namespace ChatNet;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
public class MainActivity : MauiAppCompatActivity
{
    public static MainActivity MainActivityInstance { get; private set; }



    //public IMediator Mediator { get; set; }
    //public MainActivity()
    //{

    //}

    private MediaProjectionManager mediaProjectionManager = null!;

    private const int RECORD_AUDIO_PERMISSION_REQUEST_CODE = 42;
    private const int MEDIA_PROJECTION_REQUEST_CODE = 13;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
        Microsoft.Maui.Essentials.Platform.Init(this, savedInstanceState);
        MainActivityInstance = this;
    }

    protected override void OnStart()
    {
        base.OnStart();


        if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
        {
            RequestPermissions(new string[] { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation }, 0);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Location Permission Granted!!!");
        }
    }

    public void StartCapturing()
    {
        if (!IsRecordAudioPermissionGranted())
        {
            RequestRecordAudioPermission();
        }
        else
        {
            StartMediaProjectionCapturingRequest();
        }
    }

    public void StopCapturing()
    {
        StartService(new Intent(this, Java.Lang.Class.FromType(typeof(AudioCaptureService))).SetAction(AudioCaptureService.ACTION_STOP));
    }


    public void StartTransmitting()
    {
        if (!IsRecordAudioPermissionGranted())
        {
            RequestRecordAudioPermission();
        }
        else
        {
            StartMediaProjectionTransmittingRequest();
        }
    }

    public void StopTransmitting()
    {
        StartService(new Intent(this, Java.Lang.Class.FromType(typeof(AudioTransmitterService))).SetAction(AudioTransmitterService.ACTION_STOP));
    }

    private bool IsRecordAudioPermissionGranted()
    {
        return CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted;
    }

    private void RequestRecordAudioPermission()
    {
        RequestPermissions(new string[] { Manifest.Permission.RecordAudio }, RECORD_AUDIO_PERMISSION_REQUEST_CODE);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
    {
        Microsoft.Maui.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == RECORD_AUDIO_PERMISSION_REQUEST_CODE)
        {
            if (grantResults.FirstOrDefault() == Permission.Granted)
            {
                Toast.MakeText(this, "Permissions to capture audio granted. Click the button once again.", ToastLength.Short)
                     .Show();
            }
            else
            {
                Toast.MakeText(this, "Permissions to capture audio denied.", ToastLength.Long).Show();
            }
        }
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    private void StartMediaProjectionCapturingRequest()
    {
        // use applicationContext to avoid memory leak on Android 10.
        // see: https://partnerissuetracker.corp.google.com/issues/139732252
        mediaProjectionManager = GetSystemService(MediaProjectionService).JavaCast<MediaProjectionManager>();
        StartActivityForResult(mediaProjectionManager.CreateScreenCaptureIntent(), MEDIA_PROJECTION_REQUEST_CODE);
    }

    private void StartMediaProjectionTransmittingRequest()
    {
        // use applicationContext to avoid memory leak on Android 10.
        // see: https://partnerissuetracker.corp.google.com/issues/139732252
        mediaProjectionManager = GetSystemService(MediaProjectionService).JavaCast<MediaProjectionManager>();
        StartActivityForResult(mediaProjectionManager.CreateScreenCaptureIntent(), 69);
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode == 69)
        {
            if (resultCode == Result.Ok)
            {
                Toast.MakeText(
                    this,
                    "MediaProjection permission obtained. Foreground service will be started to transmit audio.",
                    ToastLength.Short
                ).Show();

                var audioTransmitIntent = new Intent(this, typeof(AudioTransmitterService))
                    .SetAction(AudioTransmitterService.ACTION_START)
                    .PutExtra(AudioTransmitterService.EXTRA_RESULT_DATA, data!);

                //StartActivity(new Intent(this, typeof(AudioCaptureStartIntentService)));

                ContextCompat.StartForegroundService(this, audioTransmitIntent);
            }
            else
            {
                Toast.MakeText(
                    this, "Request to obtain MediaProjection denied.",
                    ToastLength.Short
                ).Show();
            }
        }
        else if (requestCode == MEDIA_PROJECTION_REQUEST_CODE)
        {
            if (resultCode == Result.Ok)
            {
                Toast.MakeText(
                    this,
                    "MediaProjection permission obtained. Foreground service will be started to capture audio.",
                    ToastLength.Short
                ).Show();

                var audioCaptureIntent = new Intent(this, typeof(AudioCaptureService))
                    .SetAction(AudioCaptureService.ACTION_START)
                    .PutExtra(AudioCaptureService.EXTRA_RESULT_DATA, data!);

                //StartActivity(new Intent(this, typeof(AudioCaptureStartIntentService)));

                ContextCompat.StartForegroundService(this, audioCaptureIntent);
            }
            else
            {
                Toast.MakeText(
                    this, "Request to obtain MediaProjection denied.",
                    ToastLength.Short
                ).Show();
            }
        }
    }


    //public MainActivity(IMediator mediator)
    //{
    //    Mediator = mediator;
    //}

    //public override void OnConfigurationChanged(Configuration newConfig)
    //{
    //    Mediator.Publish<MILOA.Services.Mediatr.OrientationChangedMsg>
    //        (new Services.Mediatr.OrientationChangedMsg
    //        { Orientation = newConfig.Orientation == Orientation.Landscape ? DeviceOrientationKind.Landscape : DeviceOrientationKind.Portrait
    //        });
    //}
}
