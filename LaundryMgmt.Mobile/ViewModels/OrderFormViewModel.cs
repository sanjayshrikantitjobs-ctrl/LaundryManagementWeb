using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class OrderFormViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly Dictionary<string, (PricingType PricingType, decimal Price)> _priceLookup = new();

    private List<GarmentListItem> _garments = new();
    private List<ServiceListItem> _services = new();

    public ObservableCollection<CustomerListItem> Customers { get; } = new();
    public ObservableCollection<OrderItemRowViewModel> Items { get; } = new();
    public OrderChannel[] Channels { get; } = Enum.GetValues<OrderChannel>();

    [ObservableProperty] private CustomerListItem? selectedCustomer;
    [ObservableProperty] private OrderChannel channel = OrderChannel.WalkIn;
    [ObservableProperty] private bool isExpress;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string subtotalDisplay = "₹0.00";

    public OrderFormViewModel(ApiClient apiClient) => _apiClient = apiClient;

    public async Task InitializeAsync()
    {
        var customers = await _apiClient.GetCustomersAsync(pageSize: 100);
        Customers.Clear();
        foreach (var customer in customers?.Items ?? new List<CustomerListItem>())
            Customers.Add(customer);

        var garments = await _apiClient.GetGarmentsAsync();
        _garments = garments?.Items ?? new List<GarmentListItem>();

        var services = await _apiClient.GetServicesAsync();
        _services = services?.Items ?? new List<ServiceListItem>();

        var matrix = await _apiClient.GetPricingMatrixAsync();
        _priceLookup.Clear();
        foreach (var row in matrix?.Garments ?? new List<PricingMatrixGarmentRowDto>())
            foreach (var cell in row.Prices)
                if (cell.Price is not null && cell.PricingType is not null)
                    _priceLookup[Key(row.GarmentId, cell.ServiceId)] = (cell.PricingType.Value, cell.Price.Value);

        Items.Clear();
        AddItem();
    }

    [RelayCommand]
    private void AddItem()
    {
        var row = new OrderItemRowViewModel(_garments, _services, LookupPrice);
        row.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(OrderItemRowViewModel.LineTotal)) RecalculateSubtotal();
        };
        Items.Add(row);
    }

    [RelayCommand]
    private void RemoveItem(OrderItemRowViewModel? row)
    {
        if (row is null || Items.Count <= 1) return;
        Items.Remove(row);
        RecalculateSubtotal();
    }

    [ObservableProperty] private bool hasMissingPrices;

    [RelayCommand]
    private async Task PlaceOrderAsync()
    {
        if (SelectedCustomer is null)
        {
            ErrorMessage = "Select a customer.";
            return;
        }

        var rowsWithSelection = Items.Where(i => i.SelectedGarment is not null && i.SelectedService is not null).ToList();
        if (rowsWithSelection.Count == 0)
        {
            ErrorMessage = "Add at least one item with a garment and service selected.";
            return;
        }

        if (rowsWithSelection.Any(i => i.LineTotal is null))
        {
            HasMissingPrices = true;
            ErrorMessage = "One or more items have no price configured for that garment + service combination. Set a price first.";
            return;
        }

        HasMissingPrices = false;
        var itemRequests = rowsWithSelection.Select(i => i.ToRequest()!).ToList();

        IsSaving = true;
        ErrorMessage = null;

        try
        {
            var response = await _apiClient.CreateOrderAsync(
                new CreateOrderRequest(SelectedCustomer.Id, Channel, IsExpress, itemRequests));

            if (response.IsSuccessStatusCode)
                await Shell.Current.GoToAsync("..");
            else
                ErrorMessage = "Failed to create order. Check that every item has a price configured.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't reach the server: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task GoToPricingAsync() => await Shell.Current.GoToAsync("//pricing");

    private (PricingType, decimal)? LookupPrice(Guid garmentId, Guid serviceId) =>
        _priceLookup.TryGetValue(Key(garmentId, serviceId), out var value) ? value : null;

    private void RecalculateSubtotal() =>
        SubtotalDisplay = $"₹{Items.Sum(i => i.LineTotal ?? 0):0.00}";

    private static string Key(Guid garmentId, Guid serviceId) => $"{garmentId}:{serviceId}";
}
