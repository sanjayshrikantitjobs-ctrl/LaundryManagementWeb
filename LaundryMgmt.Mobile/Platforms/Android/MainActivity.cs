using Android.App;
using Android.Content.PM;
using Android.OS;

namespace LaundryMgmt.Mobile;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    // SingleTask so tapping a push notification reuses this running instance instead
    // of spawning a duplicate on top of it.
    LaunchMode = LaunchMode.SingleTask,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
