using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class CustomersViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<CustomerListItem> Customers { get; } = new();

    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private string? errorMessage;

    public CustomersViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsRefreshing = true;
        ErrorMessage = null;
        try
        {
            var result = await _apiClient.GetCustomersAsync(search: SearchText);
            Customers.Clear();
            foreach (var customer in result?.Items ?? new List<CustomerListItem>())
                Customers.Add(customer);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load customers: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task NewCustomerAsync() => await Shell.Current.GoToAsync(nameof(Views.CustomerFormPage));

    [RelayCommand]
    private async Task DeleteCustomerAsync(CustomerListItem? customer)
    {
        if (customer is null) return;
        var confirmed = await Shell.Current.DisplayAlert("Delete customer", $"Delete \"{customer.FullName}\"?", "Delete", "Cancel");
        if (!confirmed) return;

        await _apiClient.DeleteCustomerAsync(customer.Id);
        await RefreshAsync();
    }
}
