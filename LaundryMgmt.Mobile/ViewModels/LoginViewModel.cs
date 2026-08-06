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
        catch (Exception)
        {
            ErrorMessage = "Invalid username or password.";
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}
