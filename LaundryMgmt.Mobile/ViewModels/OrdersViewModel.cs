using System.Collections.ObjectModel;
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

    [RelayCommand]
    private async Task NewOrderAsync() => await Shell.Current.GoToAsync(nameof(Views.OrderFormPage));

    [RelayCommand]
    private async Task OpenOrderAsync(OrderListItem? order)
    {
        if (order is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.OrderDetailPage)}?orderId={order.Id}");
    }
}
