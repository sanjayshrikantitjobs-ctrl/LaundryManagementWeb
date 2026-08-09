using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Shared.Auth;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class VerifyOtpViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    [ObservableProperty] private string phoneNumber = string.Empty;
    [ObservableProperty] private string code = string.Empty;
    [ObservableProperty] private bool isSubmitting;
    [ObservableProperty] private string? errorMessage;

    public event Action? Verified;

    public VerifyOtpViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    [RelayCommand]
    private async Task VerifyAsync()
    {
        if (string.IsNullOrWhiteSpace(Code) || Code.Length != 6)
        {
            ErrorMessage = "Enter the 6-digit code we sent to your WhatsApp.";
            return;
        }

        IsSubmitting = true;
        ErrorMessage = null;

        try
        {
            var session = await _apiClient.VerifyOtpAsync(new VerifyOtpRequest(PhoneNumber, Code));
            await _authService.ApplySessionAsync(session);
            Verified?.Invoke();
        }
        catch (Exception)
        {
            ErrorMessage = "That code is invalid or has expired.";
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}
