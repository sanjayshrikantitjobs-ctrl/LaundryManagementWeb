using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels.Paging;

namespace LaundryMgmt.Mobile.ViewModels;

/// <summary>Shared by both PickupAgent and DeliveryAgent — a session is always
/// exactly one of the two roles, so there's no simultaneity concern in branching
/// on role here (same precedent as OrdersViewModel's IsCustomer branch). AppShell
/// sets the tab label ("My Pickup Queue" vs "My Delivery Queue") per role.</summary>
public partial class OrderQueueViewModel : PagedListViewModel<MyPickupDeliveryDto>
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public ObservableCollection<MyPickupDeliveryDto> AssignedOrders => Items;

    public bool IsPickupAgent => _authService.Role == "PickupAgent";

    public OrderQueueViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    protected override Task<PaginatedList<MyPickupDeliveryDto>?> FetchPageAsync(int pageNumber, int pageSize) =>
        _apiClient.GetMyPickupDeliveriesAsync(pageNumber, pageSize);

    [RelayCommand]
    private async Task OpenOrderAsync(MyPickupDeliveryDto? order)
    {
        if (order is null) return;

        var idParam = IsPickupAgent ? "pickupId" : "deliveryId";
        await Shell.Current.GoToAsync($"{nameof(Views.OrderDetailPage)}?orderId={order.OrderId}&{idParam}={order.PickupDeliveryId}");
    }
}
