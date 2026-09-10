using LaundryMgmt.Shared.Auth;
using Plugin.FirebasePushNotifications;

namespace LaundryMgmt.Mobile.Services;

/// <summary>
/// Wraps login + session state: the JWT lives in secure storage (via
/// <see cref="AuthTokenStore"/>), the non-sensitive profile bits (name/role/userId)
/// live in Preferences so the app can show "who's logged in" and pick the right
/// Shell tabs without an extra round trip on startup.
/// </summary>
public class AuthService
{
    private const string FullNameKey = "auth_full_name";
    private const string RoleKey = "auth_role";
    private const string UserIdKey = "auth_user_id";

    private readonly ApiClient _apiClient;
    private readonly AuthTokenStore _tokenStore;
    private readonly CartService _cartService;
    private readonly IFirebasePushNotification _pushNotification;
    private readonly INotificationPermissions _notificationPermissions;

    public AuthService(
        ApiClient apiClient, AuthTokenStore tokenStore, CartService cartService,
        IFirebasePushNotification pushNotification, INotificationPermissions notificationPermissions)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
        _cartService = cartService;
        _pushNotification = pushNotification;
        _notificationPermissions = notificationPermissions;

        // A token can rotate at any time (not just on our own registration call) —
        // re-send it whenever that happens, for whoever's currently logged in.
        _pushNotification.TokenRefreshed += async (_, e) => await RegisterPushTokenAsync(e.Token);
    }

    public string? FullName => Preferences.Default.Get<string?>(FullNameKey, null);
    public string? Role => Preferences.Default.Get<string?>(RoleKey, null);
    public string? UserId => Preferences.Default.Get<string?>(UserIdKey, null);

    /// <summary>Call once at startup to restore an existing session's bearer token onto the ApiClient.</summary>
    public async Task<bool> TryRestoreSessionAsync()
    {
        var token = await _tokenStore.GetTokenAsync();
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(Role))
            return false;

        _apiClient.SetBearerToken(token);
        _cartService.SetCurrentUser(UserId);

        // Covers FCM token rotation for a customer who stays logged in across app
        // restarts without ever hitting LoginAsync/ApplySessionAsync again.
        _ = RegisterPushTokenAsync();

        return true;
    }

    public async Task<LoginResponse> LoginAsync(string usernameOrEmail, string password)
    {
        var response = await _apiClient.LoginAsync(new LoginRequest(usernameOrEmail, password))
            ?? throw new InvalidOperationException("Login failed.");

        await ApplySessionAsync(response);
        return response;
    }

    /// <summary>Persists a session that already came back from the server — used after
    /// OTP verification, which returns the same login payload shape as /login.</summary>
    public async Task ApplySessionAsync(LoginResponse response)
    {
        await _tokenStore.SaveTokenAsync(response.AccessToken);
        Preferences.Default.Set(FullNameKey, response.FullName);
        Preferences.Default.Set(RoleKey, response.Role);
        Preferences.Default.Set(UserIdKey, response.UserId);

        _apiClient.SetBearerToken(response.AccessToken);
        _cartService.SetCurrentUser(response.UserId);

        _ = RegisterPushTokenAsync();
    }

    /// <summary>Android-only for now (see LaundryMgmt.Mobile.csproj/AndroidManifest —
    /// no GoogleService-Info.plist/iOS wiring yet). Fire-and-forget by design: a push
    /// registration failure (no network, permission denied, emulator without Play
    /// Services) should never block login.</summary>
    private async Task RegisterPushTokenAsync(string? token = null)
    {
        if (DeviceInfo.Platform != DevicePlatform.Android) return;

        try
        {
            if (token is null)
            {
                // Android 13+ shows nothing unless the user has granted POST_NOTIFICATIONS
                // at runtime — the plugin doesn't request this itself, so this has to
                // happen before/alongside registration or the token would be pointless.
                await _notificationPermissions.RequestPermissionAsync();

                await _pushNotification.RegisterForPushNotificationsAsync();
                token = _pushNotification.Token;
            }

            if (!string.IsNullOrEmpty(token))
            {
                var response = await _apiClient.RegisterDeviceTokenAsync(token, "Android");
                System.Diagnostics.Debug.WriteLine(
                    $"[Push] RegisterDeviceTokenAsync -> {(int)response.StatusCode} {response.StatusCode}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[Push] No FCM token available to register.");
            }
        }
        catch (Exception ex)
        {
            // Best-effort — see summary above. Logged (not swallowed silently) so a
            // failed registration is visible in logcat/Output instead of just showing
            // up as "push never arrives" with no diagnostic trail.
            System.Diagnostics.Debug.WriteLine($"[Push] RegisterPushTokenAsync failed: {ex}");
        }
    }

    public void Logout()
    {
        _tokenStore.ClearToken();
        Preferences.Default.Remove(FullNameKey);
        Preferences.Default.Remove(RoleKey);
        Preferences.Default.Remove(UserIdKey);
        _apiClient.SetBearerToken(null);
        _cartService.SetCurrentUser(null);
    }
}
