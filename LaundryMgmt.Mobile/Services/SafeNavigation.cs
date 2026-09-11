namespace LaundryMgmt.Mobile.Services;

/// <summary>Serializes every Shell navigation in the app through one gate. Two overlapping
/// Shell.Current.GoToAsync calls — most commonly a double-tap on a row/button with no
/// debounce — can race Android's FragmentManager mid-transition and crash with
/// "Java.Lang.IllegalArgumentException: No view found for id ... (jumpToEnd) for fragment
/// NavigationRootManager...", a known MAUI/Android Shell issue. Routing every call through
/// this single semaphore means a second GoToAsync fired while one is still in flight is
/// just dropped instead of racing, eliminating the crash by construction.</summary>
public static class SafeNavigation
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public static async Task GoToAsync(string route)
    {
        if (Shell.Current is null) return;
        if (!await Gate.WaitAsync(0)) return;
        try
        {
            await Shell.Current.GoToAsync(route);
        }
        finally
        {
            Gate.Release();
        }
    }
}
