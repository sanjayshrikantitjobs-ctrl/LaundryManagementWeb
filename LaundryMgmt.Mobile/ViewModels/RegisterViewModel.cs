using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Shared.Auth;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string phoneNumber = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private bool isSubmitting;
    [ObservableProperty] private string? errorMessage;

    /// <summary>Fired with the registered phone number once the account is created,
    /// so the page can navigate to OTP verification.</summary>
    public event Action<string>? Registered;

    public RegisterViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(PhoneNumber) ||
            string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "All fields are required.";
            return;
        }

        if (Password.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters.";
            return;
        }

        IsSubmitting = true;
        ErrorMessage = null;

        try
        {
            var response = await _apiClient.RegisterAsync(new RegisterRequest(FullName, PhoneNumber, Email, Password));
            if (response.IsSuccessStatusCode)
            {
                Registered?.Invoke(PhoneNumber);
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                ErrorMessage = string.IsNullOrWhiteSpace(body)
                    ? "Registration failed. The phone number may already be registered."
                    : body;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't reach the server: {ex.Message}";
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}
