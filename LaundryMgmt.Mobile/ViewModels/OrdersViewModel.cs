using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class OrdersViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<Models.OrderListItem> Orders { get; } = new();

    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private bool isEmpty;

    public OrdersViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            var result = await _apiClient.GetOrdersAsync();
            Orders.Clear();
            foreach (var order in result?.Items ?? new List<Models.OrderListItem>())
                Orders.Add(order);
            IsEmpty = Orders.Count == 0;
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task NewOrderAsync() => await Shell.Current.GoToAsync(nameof(Views.OrderFormPage));
}
