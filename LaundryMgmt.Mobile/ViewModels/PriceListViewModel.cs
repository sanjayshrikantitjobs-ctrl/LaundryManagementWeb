using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public record PriceListCell(string ServiceName, string DisplayPrice, string DisplayEta);

public record PriceListGarmentRow(string GarmentName, Guid CategoryId, string CategoryName, List<PriceListCell> Cells);

/// <summary>Read-only garment x service pricing grid. Mirrors price-list.component.ts:
/// the pricing matrix is fetched once — switching the channel (WalkIn/PickupRequest/Express)
/// only changes how prices/ETAs are displayed, never triggers a refetch. WalkIn and
/// PickupRequest show identical numbers; only Express adds the surcharge/uses the express ETA.
/// Switching the category filters both the columns (services) and rows (garments) shown,
/// same as the admin Pricing Matrix.</summary>
public partial class PriceListViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private PricingMatrixDto? _matrix;
    private Dictionary<Guid, ServiceListItem> _servicesById = new();

    public ObservableCollection<PricingMatrixCategoryOption> Categories { get; } = new();
    public ObservableCollection<PriceListGarmentRow> Rows { get; } = new();
    public OrderChannel[] Channels { get; } = Enum.GetValues<OrderChannel>();

    [ObservableProperty] private PricingMatrixCategoryOption? selectedCategory;
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

            Categories.Clear();
            var seen = new HashSet<Guid>();
            foreach (var service in _matrix?.Services ?? new List<PricingMatrixServiceDto>())
            {
                if (seen.Add(service.CategoryId))
                    Categories.Add(new PricingMatrixCategoryOption(service.CategoryId, service.CategoryName));
            }

            if (Categories.Count > 0)
                SelectedCategory = Categories[0];
            else
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
    partial void OnSelectedCategoryChanged(PricingMatrixCategoryOption? value) => RebuildRows();

    private void RebuildRows()
    {
        Rows.Clear();
        if (_matrix is null || SelectedCategory is null) return;

        var isExpress = SelectedChannel == OrderChannel.Express;
        var categoryServices = _matrix.Services.Where(s => s.CategoryId == SelectedCategory.CategoryId).ToList();

        foreach (var garmentRow in _matrix.Garments)
        {
            var cells = new List<PriceListCell>();

            foreach (var service in categoryServices)
            {
                var cell = garmentRow.Prices.FirstOrDefault(c => c.ServiceId == service.Id);
                if (cell is not { PricingType: { } pricingType, Price: { } basePrice }) continue;
                if (!_servicesById.TryGetValue(service.Id, out var serviceInfo)) continue;

                var price = basePrice + (isExpress ? serviceInfo.ExpressSurcharge : 0);
                var etaHours = isExpress ? serviceInfo.ExpressEtaHours : serviceInfo.EstimatedTimeHours;
                var unit = pricingType == PricingType.WeightBased ? "/kg" : "/pc";

                cells.Add(new PriceListCell(service.Name, $"₹{price:0.00}{unit}", $"{etaHours}h"));
            }

            // Only rows with at least one priced cell in this category — a garment
            // matching the category but with nothing priced yet would just show dashes.
            if (cells.Count > 0)
                Rows.Add(new PriceListGarmentRow(garmentRow.GarmentName, garmentRow.CategoryId, garmentRow.CategoryName, cells));
        }
    }
}
