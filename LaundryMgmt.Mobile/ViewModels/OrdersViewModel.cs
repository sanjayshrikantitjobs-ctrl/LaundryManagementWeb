using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class OrdersViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public ObservableCollection<Models.OrderListItem> Orders { get; } = new();

    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private bool isEmpty;
    [ObservableProperty] private string? errorMessage;

    private bool IsCustomer => _authService.Role == "Customer";

    public OrdersViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsRefreshing = true;
        ErrorMessage = null;
        try
        {
            var result = IsCustomer ? await _apiClient.GetMyOrdersAsync() : await _apiClient.GetOrdersAsync();
            Orders.Clear();
            foreach (var order in result?.Items ?? new List<Models.OrderListItem>())
                Orders.Add(order);
            IsEmpty = Orders.Count == 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load orders: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task NewOrderAsync() => await Shell.Current.GoToAsync(nameof(Views.OrderFormPage));
}
