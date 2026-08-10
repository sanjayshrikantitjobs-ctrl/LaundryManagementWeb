using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

/// <summary>One garment x service cell — mirrors web's pricing-matrix.component.ts.
/// Unlike the customer-facing Price List, this shows every service per garment
/// (including combinations with no price set yet) and is tap-to-edit.</summary>
public partial class PricingMatrixCell : ObservableObject
{
    public Guid GarmentId { get; }
    public string GarmentName { get; }
    public Guid ServiceId { get; }
    public string ServiceName { get; }

    [ObservableProperty] private PricingType? pricingType;
    [ObservableProperty] private decimal? price;

    public string DisplayText => Price is decimal p
        ? $"₹{p:0.00}{(PricingType == Models.PricingType.WeightBased ? "/kg" : "")}"
        : "Set price";

    public PricingMatrixCell(Guid garmentId, string garmentName, Guid serviceId, string serviceName, PricingType? pricingType, decimal? price)
    {
        GarmentId = garmentId;
        GarmentName = garmentName;
        ServiceId = serviceId;
        ServiceName = serviceName;
        this.pricingType = pricingType;
        this.price = price;
    }

    partial void OnPriceChanged(decimal? value) => OnPropertyChanged(nameof(DisplayText));
    partial void OnPricingTypeChanged(PricingType? value) => OnPropertyChanged(nameof(DisplayText));
}

public record PricingMatrixRow(Guid GarmentId, string GarmentName, string Category, List<PricingMatrixCell> Cells);

public partial class PricingMatrixViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public ObservableCollection<PricingMatrixRow> Rows { get; } = new();

    /// <summary>Customers and Department Head can view the matrix but not edit it
    /// (also enforced server-side — GarmentsController's price endpoint is
    /// AppRoles.ManagementRoles-only, which excludes DepartmentHead).</summary>
    public bool CanEditPrice => _authService.Role is not ("Customer" or "DepartmentHead");

    [ObservableProperty] private bool isLoading = true;
    [ObservableProperty] private string? errorMessage;

    public PricingMatrixViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var matrix = await _apiClient.GetPricingMatrixAsync();
            Rows.Clear();

            foreach (var garmentRow in matrix?.Garments ?? new List<PricingMatrixGarmentRowDto>())
            {
                var cells = (matrix!.Services).Select(service =>
                {
                    var existing = garmentRow.Prices.FirstOrDefault(c => c.ServiceId == service.Id);
                    return new PricingMatrixCell(garmentRow.GarmentId, garmentRow.GarmentName, service.Id, service.Name,
                        existing?.PricingType, existing?.Price);
                }).ToList();

                Rows.Add(new PricingMatrixRow(garmentRow.GarmentId, garmentRow.GarmentName, garmentRow.Category, cells));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load the pricing matrix: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task EditCellAsync(PricingMatrixCell? cell)
    {
        if (cell is null || !CanEditPrice) return;

        var typeChoice = await Shell.Current.DisplayActionSheet(
            $"{cell.GarmentName} — {cell.ServiceName}", "Cancel", null, "Per Item", "Weight Based (per kg)");
        if (typeChoice is null or "Cancel") return;

        var newType = typeChoice == "Weight Based (per kg)" ? PricingType.WeightBased : PricingType.PerItem;

        var priceInput = await Shell.Current.DisplayPromptAsync(
            "Set price", $"Price for {cell.GarmentName} — {cell.ServiceName}",
            initialValue: cell.Price?.ToString("0.##") ?? string.Empty, keyboard: Keyboard.Numeric, accept: "Save", cancel: "Cancel");

        if (string.IsNullOrWhiteSpace(priceInput) || !decimal.TryParse(priceInput, out var newPrice) || newPrice < 0)
            return;

        try
        {
            var response = await _apiClient.SetGarmentPriceAsync(cell.GarmentId, cell.ServiceId, newType, newPrice);
            if (response.IsSuccessStatusCode)
            {
                cell.PricingType = newType;
                cell.Price = newPrice;
            }
            else
            {
                ErrorMessage = "Failed to save the price.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't reach the server: {ex.Message}";
        }
    }
}
