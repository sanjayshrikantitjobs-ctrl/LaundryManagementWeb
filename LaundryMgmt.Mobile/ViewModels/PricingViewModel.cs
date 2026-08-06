using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

/// <summary>
/// Simplified single-pair pricing editor (pick a garment + service, view/set its
/// price) rather than the full crosstab grid the web admin shows — a scrollable
/// matrix doesn't translate well to a phone screen. Use the web app for reviewing
/// the whole pricing table at once.
/// </summary>
public partial class PricingViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<GarmentListItem> Garments { get; } = new();
    public ObservableCollection<ServiceListItem> Services { get; } = new();
    public PricingType[] PricingTypes { get; } = Enum.GetValues<PricingType>();

    [ObservableProperty] private GarmentListItem? selectedGarment;
    [ObservableProperty] private ServiceListItem? selectedService;
    [ObservableProperty] private PricingType pricingType = PricingType.PerItem;
    [ObservableProperty] private string priceText = string.Empty;
    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private bool isBusy;

    public PricingViewModel(ApiClient apiClient) => _apiClient = apiClient;

    public async Task InitializeAsync()
    {
        var garments = await _apiClient.GetGarmentsAsync();
        Garments.Clear();
        foreach (var garment in garments?.Items ?? new List<GarmentListItem>())
            Garments.Add(garment);

        var services = await _apiClient.GetServicesAsync();
        Services.Clear();
        foreach (var service in services?.Items ?? new List<ServiceListItem>())
            Services.Add(service);
    }

    partial void OnSelectedGarmentChanged(GarmentListItem? value) => _ = LoadCurrentPriceAsync();
    partial void OnSelectedServiceChanged(ServiceListItem? value) => _ = LoadCurrentPriceAsync();

    private async Task LoadCurrentPriceAsync()
    {
        StatusMessage = null;
        if (SelectedGarment is null || SelectedService is null)
        {
            PriceText = string.Empty;
            return;
        }

        var detail = await _apiClient.GetGarmentByIdAsync(SelectedGarment.Id);
        var existing = detail?.ServicePrices.FirstOrDefault(p => p.ServiceId == SelectedService.Id);

        PriceText = existing is not null ? existing.Price.ToString("0.##") : string.Empty;
        PricingType = existing?.PricingType ?? PricingType.PerItem;
    }

    [RelayCommand]
    private async Task SavePriceAsync()
    {
        if (SelectedGarment is null || SelectedService is null)
        {
            StatusMessage = "Select a garment and service.";
            return;
        }

        if (!decimal.TryParse(PriceText, out var price) || price < 0)
        {
            StatusMessage = "Enter a valid price.";
            return;
        }

        IsBusy = true;
        try
        {
            var response = await _apiClient.SetGarmentPriceAsync(SelectedGarment.Id, SelectedService.Id, PricingType, price);
            StatusMessage = response.IsSuccessStatusCode ? "Price saved." : "Failed to save price.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
