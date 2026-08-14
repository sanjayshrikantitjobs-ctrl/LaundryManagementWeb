using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

/// <summary>A single slide in the Shop page hero carousel — content-only (no photography
/// assets in the app), so each slide pairs an emoji "icon" with a short benefit pitch.
/// Kept identical in spirit to client-web's shop hero carousel (shop.component.ts) so the
/// two apps read as one product.</summary>
public record HeroSlide(string Icon, string Title, string Subtitle);

/// <summary>One row in the garment grid for the currently-selected service — wraps
/// the garment with its resolved price/pricing-type for that service. For non-weight-based
/// garments, CartQuantity mirrors the live cart quantity for this garment+service pair
/// (kept in sync by ShopViewModel), driving the Add-to-Cart-vs-stepper toggle in the UI.</summary>
public partial class ShopGarmentRow : ObservableObject
{
    public GarmentListItem Garment { get; }
    public Guid ServiceId { get; }
    public string ServiceName { get; }
    public decimal ExpressSurcharge { get; }
    public PricingType PricingType { get; }
    public decimal Price { get; }
    public bool IsWeightBased => PricingType == PricingType.WeightBased;
    public string PriceLabel => IsWeightBased ? $"₹{Price:0.00}/kg" : $"₹{Price:0.00}";

    [ObservableProperty] private string weightKgText = string.Empty;
    [ObservableProperty] private int cartQuantity;
    public bool IsInCart => CartQuantity > 0;

    public ShopGarmentRow(GarmentListItem garment, ServiceListItem service, PricingType pricingType, decimal price)
    {
        Garment = garment;
        ServiceId = service.Id;
        ServiceName = service.Name;
        ExpressSurcharge = service.ExpressSurcharge;
        PricingType = pricingType;
        Price = price;
    }

    partial void OnCartQuantityChanged(int value) => OnPropertyChanged(nameof(IsInCart));
}

public partial class ShopViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly CartService _cartService;

    private List<ServiceListItem> _services = new();
    private List<GarmentListItem> _garments = new();
    private Dictionary<(Guid GarmentId, Guid ServiceId), (PricingType Type, decimal Price)> _priceLookup = new();

    public ObservableCollection<ServiceListItem> Services { get; } = new();
    public ObservableCollection<ShopGarmentRow> GarmentRows { get; } = new();
    public ObservableCollection<ActivePromotionDto> Promotions { get; } = new();

    public List<HeroSlide> HeroSlides { get; } = new()
    {
        new HeroSlide("🧺", "Doorstep pickup, doorstep delivery", "Book a slot, leave your basket at the door, and we'll handle the rest — no store visits needed."),
        new HeroSlide("✨", "Care that matches every fabric", "Delicate silks, everyday cottons, or stubborn stains — cleaned the right way, every time."),
        new HeroSlide("⚡", "In a hurry? Go Express", "Same-day and rush options get your clothes back fast, without cutting corners."),
        new HeroSlide("🎁", "Subscribe once, save every cycle", "Monthly plans bundle your regular wash and dry-cleaning at a lower price.")
    };

    [ObservableProperty] private ServiceListItem? selectedService;
    [ObservableProperty] private string garmentSearch = string.Empty;
    [ObservableProperty] private bool isLoading = true;
    [ObservableProperty] private string? errorMessage;

    public int CartItemCount => _cartService.ItemCount;
    public bool HasCartItems => CartItemCount > 0;
    public bool HasPromotions => Promotions.Count > 0;

    public ShopViewModel(ApiClient apiClient, CartService cartService)
    {
        _apiClient = apiClient;
        _cartService = cartService;
        _cartService.Items.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CartItemCount));
            OnPropertyChanged(nameof(HasCartItems));
            SyncCartQuantities();
        };
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var servicesResult = await _apiClient.GetServicesAsync(pageSize: 50);
            _services = servicesResult?.Items ?? new List<ServiceListItem>();
            Services.Clear();
            foreach (var service in _services.OrderBy(s => s.Priority))
                Services.Add(service);

            var garmentsResult = await _apiClient.GetGarmentsAsync(pageSize: 200);
            _garments = garmentsResult?.Items ?? new List<GarmentListItem>();

            var matrix = await _apiClient.GetPricingMatrixAsync();
            _priceLookup = BuildPriceLookup(matrix);

            var promotions = await _apiClient.GetActivePromotionsAsync();
            Promotions.Clear();
            foreach (var promo in promotions ?? new List<ActivePromotionDto>())
                Promotions.Add(promo);
            OnPropertyChanged(nameof(HasPromotions));

            SelectedService = Services.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load the shop: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static Dictionary<(Guid, Guid), (PricingType, decimal)> BuildPriceLookup(PricingMatrixDto? matrix)
    {
        var lookup = new Dictionary<(Guid, Guid), (PricingType, decimal)>();
        foreach (var row in matrix?.Garments ?? new List<PricingMatrixGarmentRowDto>())
        foreach (var cell in row.Prices)
            if (cell.PricingType.HasValue && cell.Price.HasValue)
                lookup[(row.GarmentId, cell.ServiceId)] = (cell.PricingType.Value, cell.Price.Value);
        return lookup;
    }

    partial void OnSelectedServiceChanged(ServiceListItem? value) => RebuildGarmentRows();
    partial void OnGarmentSearchChanged(string value) => RebuildGarmentRows();

    private void RebuildGarmentRows()
    {
        GarmentRows.Clear();
        if (SelectedService is null) return;

        var term = GarmentSearch.Trim();

        foreach (var garment in _garments)
            if (_priceLookup.TryGetValue((garment.Id, SelectedService.Id), out var priced)
                && (term.Length == 0 || garment.Name.Contains(term, StringComparison.OrdinalIgnoreCase)))
                GarmentRows.Add(new ShopGarmentRow(garment, SelectedService, priced.Type, priced.Price));

        SyncCartQuantities();
    }

    private void SyncCartQuantities()
    {
        foreach (var row in GarmentRows)
        {
            var item = _cartService.Items.FirstOrDefault(i => i.GarmentId == row.Garment.Id && i.ServiceId == row.ServiceId);
            row.CartQuantity = item?.Quantity ?? 0;
        }
    }

    [RelayCommand]
    private void AddToCart(ShopGarmentRow? row)
    {
        if (row is null) return;

        decimal? weightKg = null;

        if (row.IsWeightBased)
        {
            if (!decimal.TryParse(row.WeightKgText, out var kg) || kg <= 0) return;
            weightKg = kg;
            row.WeightKgText = string.Empty;
        }

        _cartService.Add(new CartItem(
            row.Garment.Id, row.Garment.Name, row.Garment.ImageUrl, row.Garment.CategoryName,
            row.ServiceId, row.ServiceName, row.PricingType, row.Price,
            1, weightKg, row.ExpressSurcharge));

        SyncCartQuantities();
    }

    [RelayCommand]
    private void IncrementCartQuantity(ShopGarmentRow? row)
    {
        if (row is null) return;
        _cartService.UpdateQuantity(row.Garment.Id, row.ServiceId, row.CartQuantity + 1);
        SyncCartQuantities();
    }

    [RelayCommand]
    private void DecrementCartQuantity(ShopGarmentRow? row)
    {
        if (row is null) return;
        _cartService.UpdateQuantity(row.Garment.Id, row.ServiceId, row.CartQuantity - 1);
        SyncCartQuantities();
    }

    [RelayCommand]
    private async Task OpenCartAsync() => await Shell.Current.GoToAsync(nameof(Views.CartPage));
}
