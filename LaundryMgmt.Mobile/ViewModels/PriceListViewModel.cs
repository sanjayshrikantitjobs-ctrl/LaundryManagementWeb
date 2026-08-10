using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public record PriceListCell(string ServiceName, string DisplayPrice, string DisplayEta);

public record PriceListGarmentRow(string GarmentName, string Category, List<PriceListCell> Cells);

/// <summary>Read-only garment x service pricing grid. Mirrors price-list.component.ts:
/// the pricing matrix is fetched once — switching the channel (WalkIn/PickupRequest/Express)
/// only changes how prices/ETAs are displayed, never triggers a refetch. WalkIn and
/// PickupRequest show identical numbers; only Express adds the surcharge/uses the express ETA.</summary>
public partial class PriceListViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private PricingMatrixDto? _matrix;
    private Dictionary<Guid, ServiceListItem> _servicesById = new();

    public ObservableCollection<PriceListGarmentRow> Rows { get; } = new();
    public OrderChannel[] Channels { get; } = Enum.GetValues<OrderChannel>();

    [ObservableProperty] private OrderChannel selectedChannel = OrderChannel.WalkIn;
    [ObservableProperty] private bool isLoading = true;
    [ObservableProperty] private string? errorMessage;

    public PriceListViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var servicesResult = await _apiClient.GetServicesAsync(pageSize: 50);
            _servicesById = (servicesResult?.Items ?? new List<ServiceListItem>()).ToDictionary(s => s.Id);

            _matrix = await _apiClient.GetPricingMatrixAsync();
            RebuildRows();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load the price list: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedChannelChanged(OrderChannel value) => RebuildRows();

    private void RebuildRows()
    {
        Rows.Clear();
        if (_matrix is null) return;

        var isExpress = SelectedChannel == OrderChannel.Express;

        foreach (var garmentRow in _matrix.Garments)
        {
            var cells = new List<PriceListCell>();

            foreach (var cell in garmentRow.Prices)
            {
                if (cell.PricingType is not { } pricingType || cell.Price is not { } basePrice) continue;
                if (!_servicesById.TryGetValue(cell.ServiceId, out var service)) continue;

                var price = basePrice + (isExpress ? service.ExpressSurcharge : 0);
                var etaHours = isExpress ? service.ExpressEtaHours : service.EstimatedTimeHours;
                var unit = pricingType == PricingType.WeightBased ? "/kg" : "/pc";

                cells.Add(new PriceListCell(service.Name, $"₹{price:0.00}{unit}", $"{etaHours}h"));
            }

            if (cells.Count > 0)
                Rows.Add(new PriceListGarmentRow(garmentRow.GarmentName, garmentRow.Category, cells));
        }
    }
}
