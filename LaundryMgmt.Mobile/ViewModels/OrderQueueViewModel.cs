using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class AssignedOrderSummary : ObservableObject
{
    [ObservableProperty] private string orderNumber = string.Empty;
    [ObservableProperty] private string customerName = string.Empty;
    [ObservableProperty] private string address = string.Empty;
    [ObservableProperty] private string status = string.Empty;
    public Guid OrderId { get; set; }
}

public partial class OrderQueueViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<AssignedOrderSummary> AssignedOrders { get; } = new();

    [ObservableProperty] private bool isRefreshing;

    public OrderQueueViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            // TODO: replace with _apiClient.GetMyAssignedOrdersAsync() once the
            // Pickup/Delivery controller exists on the API side.
            await Task.Delay(300);
        }
        finally
        {
            IsRefreshing = false;
        }
    }
}
