using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels.Paging;

namespace LaundryMgmt.Mobile.ViewModels;

public enum OrderTab
{
    All, New, InProgress, Completed, Cancelled
}

public partial class OrdersViewModel : PagedListViewModel<OrderListItem>
{
    /// <summary>Mirrors client-web's IN_PROGRESS_ORDER_STATUSES — every pipeline step
    /// between "just placed" and "done", collapsed into one tab for a quick glance.</summary>
    private static readonly OrderStatus[] InProgressStatuses =
    {
        OrderStatus.Received, OrderStatus.Sorting, OrderStatus.Washing, OrderStatus.Drying,
        OrderStatus.Ironing, OrderStatus.Packing, OrderStatus.ReadyForDelivery, OrderStatus.OutForDelivery
    };

    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public ObservableCollection<OrderListItem> Orders => Items;

    public OrderTab ActiveTab { get; private set; } = OrderTab.All;

    // Purely decorative counts for the status filter chips (e.g. "New 4") — fetched
    // with pageSize:1 against the same endpoint FetchPageAsync already uses, just once
    // per status, so the chip strip can show all five totals at once without a new
    // API surface. Never touches ActiveTab/Items — display-only, refreshed alongside
    // the real list load.
    [ObservableProperty] private int allCount;
    [ObservableProperty] private int newCount;
    [ObservableProperty] private int inProgressCount;
    [ObservableProperty] private int completedCount;
    [ObservableProperty] private int cancelledCount;

    private bool IsCustomer => _authService.Role == "Customer";

    public OrdersViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    protected override Task<PaginatedList<OrderListItem>?> FetchPageAsync(int pageNumber, int pageSize)
    {
        var (status, statuses) = TabToParams();
        return IsCustomer
            ? _apiClient.GetMyOrdersAsync(pageNumber, pageSize, status, statuses)
            : _apiClient.GetOrdersAsync(pageNumber, pageSize, status, statuses);
    }

    private (OrderStatus? Status, string? Statuses) TabToParams() => ActiveTab switch
    {
        OrderTab.New => (OrderStatus.New, null),
        OrderTab.InProgress => (null, string.Join(',', InProgressStatuses)),
        OrderTab.Completed => (OrderStatus.Delivered, null),
        OrderTab.Cancelled => (OrderStatus.Cancelled, null),
        _ => (null, null)
    };

    [RelayCommand]
    private void SetTab(OrderTab tab)
    {
        ActiveTab = tab;
        OnPropertyChanged(nameof(ActiveTab));
        RefreshCommand.Execute(null);
    }

    /// <summary>Fetches the total count for every status tab in parallel (pageSize:1,
    /// discarding the single item) so the filter chips can show "All 12, New 4, …" all
    /// at once. Best-effort and silent on failure — a count badge failing to load
    /// should never disrupt the actual order list.</summary>
    [RelayCommand]
    public async Task RefreshCountsAsync()
    {
        try
        {
            Func<OrderStatus?, string?, Task<PaginatedList<OrderListItem>?>> fetch = IsCustomer
                ? (status, statuses) => _apiClient.GetMyOrdersAsync(1, 1, status, statuses)
                : (status, statuses) => _apiClient.GetOrdersAsync(1, 1, status, statuses);

            var inProgress = string.Join(',', InProgressStatuses);
            var results = await Task.WhenAll(
                fetch(null, null),
                fetch(OrderStatus.New, null),
                fetch(null, inProgress),
                fetch(OrderStatus.Delivered, null),
                fetch(OrderStatus.Cancelled, null));

            AllCount = results[0]?.TotalCount ?? 0;
            NewCount = results[1]?.TotalCount ?? 0;
            InProgressCount = results[2]?.TotalCount ?? 0;
            CompletedCount = results[3]?.TotalCount ?? 0;
            CancelledCount = results[4]?.TotalCount ?? 0;
        }
        catch
        {
            // Best-effort — leave whatever counts were last successfully loaded.
        }
    }

    [RelayCommand]
    private async Task NewOrderAsync() => await SafeNavigation.GoToAsync(nameof(Views.OrderFormPage));

    [RelayCommand]
    private async Task OpenOrderAsync(OrderListItem? order)
    {
        if (order is null) return;
        await SafeNavigation.GoToAsync($"{nameof(Views.OrderDetailPage)}?orderId={order.Id}");
    }
}
