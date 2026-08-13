using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class CustomerDetailViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    [ObservableProperty] private Guid? customerId;
    [ObservableProperty] private CustomerDetailDto? customer;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;

    public bool CanEditMasterData => _authService.Role is not ("Customer" or "DepartmentHead");

    public CustomerDetailViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    partial void OnCustomerIdChanged(Guid? value)
    {
        if (value is Guid id)
            _ = LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            Customer = await _apiClient.GetCustomerByIdAsync(id);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (Customer is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.CustomerFormPage)}?customerId={Customer.Id}");
    }

    [RelayCommand]
    private async Task ToggleStatusAsync()
    {
        if (Customer is null) return;

        var nextStatus = Customer.Status == CustomerStatus.Active ? CustomerStatus.Inactive : CustomerStatus.Active;
        var action = nextStatus == CustomerStatus.Inactive ? "Deactivate" : "Activate";
        var confirmed = await Shell.Current.DisplayAlert(
            $"{action} customer", $"{action} \"{Customer.FullName}\"? Their orders and history will not be affected.", action, "Cancel");
        if (!confirmed) return;

        await _apiClient.SetCustomerStatusAsync(Customer.Id, nextStatus);
        await LoadAsync(Customer.Id);
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Customer is null) return;

        var confirmed = await Shell.Current.DisplayAlert(
            "Delete customer", $"Delete \"{Customer.FullName}\"? This cannot be undone.", "Continue", "Cancel");
        if (!confirmed) return;

        var reason = await Shell.Current.DisplayPromptAsync(
            "Reason for deletion", "Why is this customer being deleted?", "Delete", "Cancel", placeholder: "Reason (optional)");
        if (reason is null) return; // cancelled

        await _apiClient.DeleteCustomerAsync(Customer.Id, reason);
        await Shell.Current.GoToAsync("..");
    }
}
