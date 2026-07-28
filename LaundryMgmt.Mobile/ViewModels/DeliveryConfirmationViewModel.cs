using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Shared.Auth;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class DeliveryConfirmationViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    [ObservableProperty] private Guid orderId;
    [ObservableProperty] private string otp = string.Empty;
    [ObservableProperty] private bool isSubmitting;
    [ObservableProperty] private string? errorMessage;

    public DeliveryConfirmationViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (string.IsNullOrWhiteSpace(Otp))
        {
            ErrorMessage = "Enter the OTP the customer received by SMS.";
            return;
        }

        IsSubmitting = true;
        ErrorMessage = null;

        try
        {
            var response = await _apiClient.ConfirmDeliveryAsync(new DeliveryOtpConfirmationRequest(OrderId, Otp));
            if (!response.IsSuccessStatusCode)
                ErrorMessage = "That OTP didn't match. Please try again.";
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
