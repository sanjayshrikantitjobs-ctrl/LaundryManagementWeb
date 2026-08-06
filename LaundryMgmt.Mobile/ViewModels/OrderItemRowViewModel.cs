using CommunityToolkit.Mvvm.ComponentModel;
using LaundryMgmt.Mobile.Models;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class OrderItemRowViewModel : ObservableObject
{
    private readonly Func<Guid, Guid, (PricingType PricingType, decimal Price)?> _priceLookup;

    public List<GarmentListItem> AvailableGarments { get; }
    public List<ServiceListItem> AvailableServices { get; }

    [ObservableProperty] private GarmentListItem? selectedGarment;
    [ObservableProperty] private ServiceListItem? selectedService;
    [ObservableProperty] private int quantity = 1;
    [ObservableProperty] private string? weightKgText;
    [ObservableProperty] private string lineTotalDisplay = "—";
    [ObservableProperty] private decimal? lineTotal;

    public OrderItemRowViewModel(
        List<GarmentListItem> availableGarments, List<ServiceListItem> availableServices,
        Func<Guid, Guid, (PricingType, decimal)?> priceLookup)
    {
        AvailableGarments = availableGarments;
        AvailableServices = availableServices;
        _priceLookup = priceLookup;
    }

    partial void OnSelectedGarmentChanged(GarmentListItem? value) => Recompute();
    partial void OnSelectedServiceChanged(ServiceListItem? value) => Recompute();
    partial void OnQuantityChanged(int value) => Recompute();
    partial void OnWeightKgTextChanged(string? value) => Recompute();

    private void Recompute()
    {
        if (SelectedGarment is null || SelectedService is null)
        {
            LineTotal = null;
            LineTotalDisplay = "—";
            return;
        }

        var price = _priceLookup(SelectedGarment.Id, SelectedService.Id);
        if (price is null)
        {
            LineTotal = null;
            LineTotalDisplay = "no price set";
            return;
        }

        var (pricingType, unitPrice) = price.Value;
        decimal total;
        if (pricingType == PricingType.WeightBased)
        {
            decimal.TryParse(WeightKgText, out var weight);
            total = unitPrice * weight;
        }
        else
        {
            total = unitPrice * Quantity;
        }

        LineTotal = total;
        LineTotalDisplay = $"₹{total:0.00}";
    }

    public CreateOrderItemRequest? ToRequest()
    {
        if (SelectedGarment is null || SelectedService is null) return null;
        decimal? weight = decimal.TryParse(WeightKgText, out var w) ? w : null;
        return new CreateOrderItemRequest(SelectedGarment.Id, SelectedService.Id, Quantity, weight, null);
    }
}
