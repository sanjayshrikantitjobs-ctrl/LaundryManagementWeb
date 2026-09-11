using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

/// <summary>A single slide in the Shop page hero carousel — real marketing photography
/// (bundled app images, not fetched from the API) rather than a plain color/icon slide,
/// so the home screen opens with an actual banner carousel.</summary>
public record HeroSlide(string ImageSource);

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

    private List<ServiceCategoryDto> _categories = new();
    private List<ServiceListItem> _allServices = new();
    private List<GarmentListItem> _garments = new();
    private Dictionary<(Guid GarmentId, Guid ServiceId), (PricingType Type, decimal Price)> _priceLookup = new();

    public ObservableCollection<ServiceCategoryDto> Categories { get; } = new();
    public ObservableCollection<ServiceListItem> Services { get; } = new();
    public ObservableCollection<ShopGarmentRow> GarmentRows { get; } = new();

    // Bundled marketing banners (Resources/Images/banner_*) — a real photo carousel
    // instead of a plain color/icon slide.
    public List<HeroSlide> HeroSlides { get; } = new()
    {
        new HeroSlide("banner_express.png"),
        new HeroSlide("banner_subscribe.png"),
        new HeroSlide("banner_care.png"),
        new HeroSlide("banner_doorstep.png"),
        new HeroSlide("banner_professional_care.jpg")
    };

    [ObservableProperty] private ServiceCategoryDto? selectedCategory;
    [ObservableProperty] private ServiceListItem? selectedService;
    [ObservableProperty] private string garmentSearch = string.Empty;
    [ObservableProperty] private bool isLoading = true;
    [ObservableProperty] private string? errorMessage;

    // Drives a full-screen "Loading garments…" overlay on ShopPage for the brief gap
    // between tapping a category and GarmentListPage actually appearing — a thin
    // top-of-screen progress bar (AppShell's global one) wasn't prominent enough for
    // customers to notice, so this is a big, impossible-to-miss dimmed overlay instead.
    [ObservableProperty] private bool isNavigatingToGarments;

    public int CartItemCount => _cartService.ItemCount;
    public bool HasCartItems => CartItemCount > 0;
    public bool HasServices => Services.Count > 0;

    // GarmentListPage's service picker default — "All" (no single service chosen)
    // shows garments from every service in the selected category.
    public bool IsAllSelected => SelectedService is null;

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
        Services.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasServices));
    }

    private bool _hasLoaded;

    // True only while InitializeAsync is assigning the *default* category — suppresses
    // the auto-navigate-to-GarmentListPage behavior in OnSelectedCategoryChanged so the
    // app doesn't jump off ShopPage the instant it finishes loading. A real tap on a
    // category card always happens with this false.
    private bool _isApplyingDefaultCategory;

    [RelayCommand]
    public async Task InitializeAsync()
    {
        // ShopViewModel is a singleton (shared with GarmentListPage so the two pages
        // browse the same in-progress selection) — OnAppearing calls this every time
        // the page is shown, including when the customer navigates *back* from
        // GarmentListPage. Without this guard that would re-fetch the whole catalogue
        // and reset SelectedCategory/SelectedService back to the defaults every time.
        if (_hasLoaded) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var categoriesResult = await _apiClient.GetServiceCategoriesAsync();
            _categories = (categoriesResult ?? new List<ServiceCategoryDto>())
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToList();
            Categories.Clear();
            foreach (var category in _categories)
                Categories.Add(category);

            var servicesResult = await _apiClient.GetServicesAsync(pageSize: 50);
            _allServices = servicesResult?.Items ?? new List<ServiceListItem>();

            var garmentsResult = await _apiClient.GetGarmentsAsync(pageSize: 200);
            _garments = garmentsResult?.Items ?? new List<GarmentListItem>();

            var matrix = await _apiClient.GetPricingMatrixAsync();
            _priceLookup = BuildPriceLookup(matrix);

            // Categories becomes populated (and tappable) right after the very first
            // of these four awaits — on a slow connection, a category tapped before
            // the rest finish gets caught by OnSelectedCategoryChanged's `!_hasLoaded`
            // guard below and silently ignored (services/garments aren't ready yet).
            // Capture it here, before the default-category assignment can overwrite
            // it, so it can be replayed for real once everything has loaded.
            var pendingTap = SelectedCategory;

            // Defaults to the first category that actually has a service, not just
            // Categories[0] — an empty category would otherwise open to a dead end.
            _isApplyingDefaultCategory = true;
            SelectedCategory ??= Categories.FirstOrDefault(c => _allServices.Any(s => s.CategoryId == c.Id));
            _isApplyingDefaultCategory = false;
            _hasLoaded = true;

            if (pendingTap is not null)
                ApplyCategorySelection(pendingTap);
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

    // Category -> Garments, with an in-between service filter: picking a category
    // populates its services and navigates straight to GarmentListPage, which opens on
    // "All" (SelectedService null) — every service in that category — and narrows to a
    // single service only if the customer taps a chip there.
    partial void OnSelectedCategoryChanged(ServiceCategoryDto? value)
    {
        Services.Clear();
        SelectedService = null;
        if (value is null) return;

        if (!_hasLoaded)
        {
            // Tapped before services/garments/pricing finished loading (see the
            // pendingTap capture in InitializeAsync) — can't filter or navigate yet
            // with empty data, and IsLoading is still true so the customer can see
            // the page is still busy. InitializeAsync replays this once it's done.
            return;
        }

        ApplyCategorySelection(value);
    }

    private void ApplyCategorySelection(ServiceCategoryDto value)
    {
        foreach (var service in _allServices.Where(s => s.CategoryId == value.Id).OrderBy(s => s.Priority))
            Services.Add(service);

        RebuildGarmentRows();

        if (!_isApplyingDefaultCategory)
            _ = NavigateToGarmentsAsync();
    }

    private async Task NavigateToGarmentsAsync()
    {
        IsNavigatingToGarments = true;
        try
        {
            // A cache-hit navigation (the common case — the catalogue is already
            // loaded) can otherwise finish inside a single UI-thread tick, before the
            // overlay ever gets a chance to actually paint a frame. One short delay is
            // enough to guarantee a render pass without adding a noticeable wait —
            // GoToAsync's own page-push transition (~250-300ms) is on top of this.
            await Task.Delay(60);
            await SafeNavigation.GoToAsync(nameof(Views.GarmentListPage));
        }
        finally
        {
            IsNavigatingToGarments = false;
        }
    }

    partial void OnSelectedServiceChanged(ServiceListItem? value)
    {
        OnPropertyChanged(nameof(IsAllSelected));
        RebuildGarmentRows();
    }

    [RelayCommand]
    private void SelectAllServices() => SelectedService = null;

    partial void OnGarmentSearchChanged(string value) => RebuildGarmentRows();

    private void RebuildGarmentRows()
    {
        GarmentRows.Clear();
        var term = GarmentSearch.Trim();

        if (term.Length > 0)
        {
            // A search should find a garment under any service/category, not just
            // whichever service chip happens to be selected — the selected service
            // only scopes browsing when the customer isn't actively searching.
            foreach (var garment in _garments)
            {
                if (!garment.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) continue;
                foreach (var service in _allServices)
                    if (_priceLookup.TryGetValue((garment.Id, service.Id), out var priced))
                        GarmentRows.Add(new ShopGarmentRow(garment, service, priced.Type, priced.Price));
            }
        }
        else if (SelectedService is not null)
        {
            foreach (var garment in _garments)
                if (_priceLookup.TryGetValue((garment.Id, SelectedService.Id), out var priced))
                    GarmentRows.Add(new ShopGarmentRow(garment, SelectedService, priced.Type, priced.Price));
        }
        else
        {
            // "All" — every service within the selected category, not every service
            // app-wide (Services is already scoped to SelectedCategory).
            foreach (var garment in _garments)
                foreach (var service in Services)
                    if (_priceLookup.TryGetValue((garment.Id, service.Id), out var priced))
                        GarmentRows.Add(new ShopGarmentRow(garment, service, priced.Type, priced.Price));
        }

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
    private async Task OpenCartAsync() => await SafeNavigation.GoToAsync(nameof(Views.CartPage));
}
