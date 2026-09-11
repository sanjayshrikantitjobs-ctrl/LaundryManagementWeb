using Android.App;
using Android.Content.PM;
using Android.OS;

namespace LaundryMgmt.Mobile;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    // SingleTop (not SingleTask) so tapping a push notification reuses this running
    // instance via OnNewIntent instead of spawning a duplicate — SingleTask went
    // further than needed and could make Android reuse the task across a launch
    // context where Shell's underlying AndroidX Navigation fragment still held view
    // references from before, throwing "No view found for id ... for fragment
    // NavigationRootManager_ElementNavigationController" on relaunch (e.g. reopening
    // from the launcher icon after the app had been backgrounded for a while).
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
