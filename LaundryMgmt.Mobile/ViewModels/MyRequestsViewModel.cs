using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

/// <summary>Simplified, non-paginated view of active orders — mirrors web's
/// my-requests.component.ts, which reuses the same /orders/mine endpoint as
/// My Orders but filters out terminal statuses client-side.</summary>
public partial class MyRequestsViewModel : ObservableObject
{
    private static readonly OrderStatus[] TerminalStatuses = { OrderStatus.Delivered, OrderStatus.Cancelled };

    private readonly ApiClient _apiClient;

    public ObservableCollection<OrderListItem> Requests { get; } = new();

    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private string? errorMessage;

    public MyRequestsViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsRefreshing = true;
        ErrorMessage = null;

        try
        {
            var result = await _apiClient.GetMyOrdersAsync(pageNumber: 1, pageSize: 100);
            Requests.Clear();
            foreach (var order in (result?.Items ?? new List<OrderListItem>()).Where(o => !TerminalStatuses.Contains(o.Status)))
                Requests.Add(order);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load your requests: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task OpenOrderAsync(OrderListItem? order)
    {
        if (order is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.OrderDetailPage)}?orderId={order.Id}");
    }
}
