using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;

namespace ChatNet;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnStart()
    {
        base.OnStart();

        if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
        {
            RequestPermissions(new string[] { Manifest.Permission.Internet }, 0);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Location Permission Granted!!!");
        }
    }
}
