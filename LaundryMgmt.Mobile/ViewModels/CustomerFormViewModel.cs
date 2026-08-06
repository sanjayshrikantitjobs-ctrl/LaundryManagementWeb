using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class CustomerFormViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string phoneNumber = string.Empty;
    [ObservableProperty] private string? email;
    [ObservableProperty] private string creditLimitText = "0";
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string? errorMessage;

    public CustomerFormViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(PhoneNumber))
        {
            ErrorMessage = "Full name and phone number are required.";
            return;
        }

        IsSaving = true;
        ErrorMessage = null;
        decimal.TryParse(CreditLimitText, out var creditLimit);

        try
        {
            var response = await _apiClient.CreateCustomerAsync(
                new CreateCustomerRequest(FullName, PhoneNumber, string.IsNullOrWhiteSpace(Email) ? null : Email, creditLimit, null));

            if (response.IsSuccessStatusCode)
                await Shell.Current.GoToAsync("..");
            else
                ErrorMessage = "Failed to save customer. The phone number may already be in use.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't reach the server: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }
}
