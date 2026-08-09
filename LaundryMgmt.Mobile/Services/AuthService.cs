using LaundryMgmt.Shared.Auth;

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

    public AuthService(ApiClient apiClient, AuthTokenStore tokenStore)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
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
    }

    public void Logout()
    {
        _tokenStore.ClearToken();
        Preferences.Default.Remove(FullNameKey);
        Preferences.Default.Remove(RoleKey);
        Preferences.Default.Remove(UserIdKey);
        _apiClient.SetBearerToken(null);
    }
}
