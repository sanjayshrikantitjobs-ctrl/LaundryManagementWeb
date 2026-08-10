using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty] private string usernameOrEmail = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private bool isSubmitting;
    [ObservableProperty] private string? errorMessage;

    public event Action? LoggedIn;

    public LoginViewModel(AuthService authService) => _authService = authService;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(UsernameOrEmail) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Enter your username and password.";
            return;
        }

        IsSubmitting = true;
        ErrorMessage = null;

        try
        {
            await _authService.LoginAsync(UsernameOrEmail, Password);
            LoggedIn?.Invoke();
        }
        catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
        {
            // A real response came back with a bad status (401 for wrong credentials,
            // etc.) — the server was reachable, the login itself failed.
            ErrorMessage = "Invalid username or password.";
        }
        catch (Exception ex)
        {
            // No HTTP status at all means the request never got a response —
            // DNS failure, connection refused, TLS handshake failure, timeout. This is
            // a connectivity problem, not a login problem; showing "Invalid username or
            // password" here would be actively misleading, so show what actually broke.
            ErrorMessage = $"Couldn't reach the server: {ex.Message}";
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}
