using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class CustomerFormViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public MembershipTier[] MembershipTiers { get; } = Enum.GetValues<MembershipTier>();

    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string phoneNumber = string.Empty;
    [ObservableProperty] private string? email;
    [ObservableProperty] private string creditLimitText = "0";
    [ObservableProperty] private MembershipTier membershipTier = MembershipTier.None;
    [ObservableProperty] private string? notes;
    [ObservableProperty] private bool isEditMode;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string? errorMessage;

    [ObservableProperty] private Guid? customerId;

    public CustomerFormViewModel(ApiClient apiClient) => _apiClient = apiClient;

    partial void OnCustomerIdChanged(Guid? value)
    {
        if (value is Guid id)
            _ = LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        IsEditMode = true;
        try
        {
            var customer = await _apiClient.GetCustomerByIdAsync(id);
            if (customer is null) return;

            FullName = customer.FullName;
            PhoneNumber = customer.PhoneNumber;
            Email = customer.Email;
            CreditLimitText = customer.CreditLimit.ToString("0.##");
            MembershipTier = customer.MembershipTier;
            Notes = customer.Notes;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load customer: {ex.Message}";
        }
    }

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
            HttpResponseMessage response;
            if (IsEditMode && CustomerId is Guid id)
            {
                response = await _apiClient.UpdateCustomerAsync(
                    id, new UpdateCustomerRequest(FullName, PhoneNumber, string.IsNullOrWhiteSpace(Email) ? null : Email, creditLimit, MembershipTier, Notes));
            }
            else
            {
                response = await _apiClient.CreateCustomerAsync(
                    new CreateCustomerRequest(FullName, PhoneNumber, string.IsNullOrWhiteSpace(Email) ? null : Email, creditLimit, Notes));
            }

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
