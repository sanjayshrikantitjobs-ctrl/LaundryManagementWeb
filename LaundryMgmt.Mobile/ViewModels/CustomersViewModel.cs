using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels.Paging;

namespace LaundryMgmt.Mobile.ViewModels;

public enum CustomerTab
{
    All, Active, Inactive
}

public partial class CustomersViewModel : PagedListViewModel<CustomerListItem>
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public ObservableCollection<CustomerListItem> Customers => Items;

    [ObservableProperty] private string searchText = string.Empty;

    public CustomerTab ActiveTab { get; private set; } = CustomerTab.All;

    public bool CanEditMasterData => _authService.Role is not ("Customer" or "DepartmentHead");

    public CustomersViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    protected override Task<PaginatedList<CustomerListItem>?> FetchPageAsync(int pageNumber, int pageSize)
    {
        var status = ActiveTab switch
        {
            CustomerTab.Active => CustomerStatus.Active,
            CustomerTab.Inactive => CustomerStatus.Inactive,
            _ => (CustomerStatus?)null
        };
        return _apiClient.GetCustomersAsync(search: SearchText, pageNumber: pageNumber, pageSize: pageSize, status: status);
    }

    [RelayCommand]
    private void SetTab(CustomerTab tab)
    {
        ActiveTab = tab;
        OnPropertyChanged(nameof(ActiveTab));
        RefreshCommand.Execute(null);
    }

    [RelayCommand]
    private async Task NewCustomerAsync() => await Shell.Current.GoToAsync(nameof(Views.CustomerFormPage));

    [RelayCommand]
    private async Task EditCustomerAsync(CustomerListItem? customer)
    {
        if (customer is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.CustomerFormPage)}?customerId={customer.Id}");
    }

    [RelayCommand]
    private async Task DeleteCustomerAsync(CustomerListItem? customer)
    {
        if (customer is null) return;
        var confirmed = await Shell.Current.DisplayAlert("Delete customer", $"Delete \"{customer.FullName}\"?", "Delete", "Cancel");
        if (!confirmed) return;

        await _apiClient.DeleteCustomerAsync(customer.Id);
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task ToggleStatusAsync(CustomerListItem? customer)
    {
        if (customer is null) return;

        var nextStatus = customer.Status == CustomerStatus.Active ? CustomerStatus.Inactive : CustomerStatus.Active;
        var action = nextStatus == CustomerStatus.Inactive ? "Deactivate" : "Activate";
        var confirmed = await Shell.Current.DisplayAlert(
            $"{action} customer", $"{action} \"{customer.FullName}\"? Their orders and history will not be affected.", action, "Cancel");
        if (!confirmed) return;

        await _apiClient.SetCustomerStatusAsync(customer.Id, nextStatus);
        await RefreshAsync();
    }
}
