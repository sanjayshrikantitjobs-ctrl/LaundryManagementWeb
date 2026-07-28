namespace LaundryMgmt.Mobile.Services;

/// <summary>Persists the JWT using platform secure storage (Keychain on iOS,
/// Keystore on Android) so tokens survive app restarts without living in
/// plain-text preferences.</summary>
public class AuthTokenStore
{
    private const string TokenKey = "access_token";

    public Task SaveTokenAsync(string token) => SecureStorage.Default.SetAsync(TokenKey, token);

    public Task<string?> GetTokenAsync() => SecureStorage.Default.GetAsync(TokenKey);

    public void ClearToken() => SecureStorage.Default.Remove(TokenKey);
}
