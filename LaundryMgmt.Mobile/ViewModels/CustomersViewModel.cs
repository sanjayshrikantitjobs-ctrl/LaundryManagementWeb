using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels.Paging;

namespace LaundryMgmt.Mobile.ViewModels;

public enum CustomerTab
{
    All, Active, Inactive, Subscribed
}

public partial class CustomersViewModel : PagedListViewModel<CustomerListItem>
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public ObservableCollection<CustomerListItem> Customers => Items;

    // Subscribed Customers is a differently-shaped list (CustomerSubscriptionListItemDto,
    // not CustomerListItem) so it can't reuse the base PagedListViewModel's Items/paging —
    // it's a simple one-shot fetch instead, mirroring the web admin's equivalent tab.
    public ObservableCollection<CustomerSubscriptionListItemDto> SubscribedCustomers { get; } = new();

    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private bool isLoadingSubscribed;

    public CustomerTab ActiveTab { get; private set; } = CustomerTab.All;
    public bool IsSubscribedTab => ActiveTab == CustomerTab.Subscribed;

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
        OnPropertyChanged(nameof(IsSubscribedTab));

        if (tab == CustomerTab.Subscribed)
            _ = LoadSubscribedAsync();
        else
            RefreshCommand.Execute(null);
    }

    private async Task LoadSubscribedAsync()
    {
        IsLoadingSubscribed = true;
        try
        {
            var result = await _apiClient.GetCustomerSubscriptionsAsync(search: SearchText, pageSize: 50);
            SubscribedCustomers.Clear();
            foreach (var item in result?.Items ?? new List<CustomerSubscriptionListItemDto>())
                SubscribedCustomers.Add(item);
        }
        finally
        {
            IsLoadingSubscribed = false;
        }
    }

    [RelayCommand]
    private async Task NewCustomerAsync() => await Shell.Current.GoToAsync(nameof(Views.CustomerFormPage));

    /// <summary>Row tap opens the read-only detail page, where Edit/Deactivate/Delete now
    /// live (replacing the old inline per-row buttons — see CustomerDetailViewModel).</summary>
    [RelayCommand]
    private async Task OpenCustomerAsync(CustomerListItem? customer)
    {
        if (customer is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.CustomerDetailPage)}?customerId={customer.Id}");
    }
}
