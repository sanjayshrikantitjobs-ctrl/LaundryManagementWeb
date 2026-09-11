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

    // Purely decorative counts for the status filter chips (e.g. "Active 12") — same
    // idea as OrdersViewModel.RefreshCountsAsync: pageSize:1 against the existing
    // endpoints, display-only, never touches ActiveTab/Items/SubscribedCustomers.
    [ObservableProperty] private int allCount;
    [ObservableProperty] private int activeCount;
    [ObservableProperty] private int inactiveCount;
    [ObservableProperty] private int subscribedCount;

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

    /// <summary>Fetches the total count for every status tab in parallel (pageSize:1,
    /// discarding the single item) so the filter chips can show "Active 12, Inactive 3,
    /// Subscribed 5" all at once. Best-effort and silent on failure.</summary>
    [RelayCommand]
    public async Task RefreshCountsAsync()
    {
        try
        {
            // Two different result shapes (CustomerListItem vs CustomerSubscriptionListItemDto)
            // so this can't use the single-type Task.WhenAll<T> overload — the plain
            // Task[] overload runs them all concurrently regardless.
            var allTask = _apiClient.GetCustomersAsync(search: SearchText, pageNumber: 1, pageSize: 1);
            var activeTask = _apiClient.GetCustomersAsync(search: SearchText, pageNumber: 1, pageSize: 1, status: CustomerStatus.Active);
            var inactiveTask = _apiClient.GetCustomersAsync(search: SearchText, pageNumber: 1, pageSize: 1, status: CustomerStatus.Inactive);
            var subscribedTask = _apiClient.GetCustomerSubscriptionsAsync(search: SearchText, pageSize: 1);

            await Task.WhenAll(allTask, activeTask, inactiveTask, subscribedTask);

            AllCount = allTask.Result?.TotalCount ?? 0;
            ActiveCount = activeTask.Result?.TotalCount ?? 0;
            InactiveCount = inactiveTask.Result?.TotalCount ?? 0;
            SubscribedCount = subscribedTask.Result?.TotalCount ?? 0;
        }
        catch
        {
            // Best-effort — leave whatever counts were last successfully loaded.
        }
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
    private async Task NewCustomerAsync() => await SafeNavigation.GoToAsync(nameof(Views.CustomerFormPage));

    /// <summary>Row tap opens the read-only detail page, where Edit/Deactivate/Delete now
    /// live (replacing the old inline per-row buttons — see CustomerDetailViewModel).</summary>
    [RelayCommand]
    private async Task OpenCustomerAsync(CustomerListItem? customer)
    {
        if (customer is null) return;
        await SafeNavigation.GoToAsync($"{nameof(Views.CustomerDetailPage)}?customerId={customer.Id}");
    }
}
