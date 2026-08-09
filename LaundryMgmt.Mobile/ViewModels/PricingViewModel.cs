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
    private readonly AuthService _authService;

    public ObservableCollection<GarmentListItem> Garments { get; } = new();
    public ObservableCollection<ServiceListItem> Services { get; } = new();
    public PricingType[] PricingTypes { get; } = Enum.GetValues<PricingType>();

    /// <summary>Customers can look up prices but not change them (also enforced
    /// server-side — GarmentsController's price endpoint is staff/admin only).</summary>
    public bool CanEditPrice => _authService.Role != "Customer";

    [ObservableProperty] private GarmentListItem? selectedGarment;
    [ObservableProperty] private ServiceListItem? selectedService;
    [ObservableProperty] private PricingType pricingType = PricingType.PerItem;
    [ObservableProperty] private string priceText = string.Empty;
    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool loadFailed;

    public PricingViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        LoadFailed = false;
        StatusMessage = null;

        try
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
        catch (Exception ex)
        {
            LoadFailed = true;
            StatusMessage = $"Couldn't load garments/services: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
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

        try
        {
            var detail = await _apiClient.GetGarmentByIdAsync(SelectedGarment.Id);
            var existing = detail?.ServicePrices.FirstOrDefault(p => p.ServiceId == SelectedService.Id);

            PriceText = existing is not null ? existing.Price.ToString("0.##") : string.Empty;
            PricingType = existing?.PricingType ?? PricingType.PerItem;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Couldn't load the current price: {ex.Message}";
        }
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
        catch (Exception ex)
        {
            StatusMessage = $"Couldn't reach the server: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
