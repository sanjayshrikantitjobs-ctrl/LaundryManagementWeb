using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public record TimeSlot(string Label, int Hour);

public partial class CartViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly CartService _cartService;

    // 2-hour pickup/delivery windows — kept identical to client-web's TIME_SLOTS
    // (shop.component.ts) so both apps offer the same options.
    public List<TimeSlot> TimeSlots { get; } = new()
    {
        new TimeSlot("11am - 1pm", 11),
        new TimeSlot("1pm - 3pm", 13),
        new TimeSlot("3pm - 5pm", 15),
        new TimeSlot("5pm - 7pm", 17)
    };

    public ObservableCollection<CartItem> Items => _cartService.Items;
    public ObservableCollection<CustomerAddressDto> Addresses { get; } = new();
    public ObservableCollection<ActivePromotionDto> Promotions { get; } = new();

    [ObservableProperty] private bool isExpress;
    [ObservableProperty] private CustomerAddressDto? selectedAddress;
    [ObservableProperty] private DateTime pickupDate = DateTime.Today;
    [ObservableProperty] private int? pickupSlotHour;
    [ObservableProperty] private DateTime deliveryDate = DateTime.Today;
    [ObservableProperty] private int? deliverySlotHour;
    [ObservableProperty] private string promoCodeText = string.Empty;
    [ObservableProperty] private ActivePromotionDto? appliedPromo;
    [ObservableProperty] private string? promoMessage;

    [ObservableProperty] private bool showAddAddressForm;
    [ObservableProperty] private string newAddressLabel = string.Empty;
    [ObservableProperty] private string newAddressLine1 = string.Empty;
    [ObservableProperty] private string? newAddressLine2;
    [ObservableProperty] private string newAddressCity = string.Empty;
    [ObservableProperty] private string newAddressState = string.Empty;
    [ObservableProperty] private string newAddressPostalCode = string.Empty;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isPlacingOrder;
    [ObservableProperty] private string? errorMessage;

    public bool HasItems => Items.Count > 0;
    public decimal Subtotal => Items.Sum(i => i.LineTotal);
    public decimal ExpressSurchargeTotal => IsExpress ? Items.Sum(i => i.ExpressSurcharge * i.Quantity) : 0;
    public decimal DiscountAmount => ComputeDiscount();
    public decimal GrandTotal => Subtotal + ExpressSurchargeTotal - DiscountAmount;

    public CartViewModel(ApiClient apiClient, CartService cartService)
    {
        _apiClient = apiClient;
        _cartService = cartService;
        Items.CollectionChanged += (_, _) => RaiseTotalsChanged();
    }

    private void RaiseTotalsChanged()
    {
        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(ExpressSurchargeTotal));
        OnPropertyChanged(nameof(DiscountAmount));
        OnPropertyChanged(nameof(GrandTotal));
    }

    partial void OnIsExpressChanged(bool value) => RaiseTotalsChanged();

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            await ReloadAddressesAsync();

            var promotions = await _apiClient.GetActivePromotionsAsync();
            Promotions.Clear();
            foreach (var promo in promotions ?? new List<ActivePromotionDto>())
                Promotions.Add(promo);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load checkout details: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ReloadAddressesAsync()
    {
        var addresses = await _apiClient.GetMyAddressesAsync();
        Addresses.Clear();
        foreach (var address in addresses ?? new List<CustomerAddressDto>())
            Addresses.Add(address);
        SelectedAddress = Addresses.FirstOrDefault(a => a.IsDefault) ?? Addresses.FirstOrDefault();
    }

    [RelayCommand]
    private void IncrementQuantity(CartItem? item)
    {
        if (item is null) return;
        _cartService.UpdateQuantity(item.GarmentId, item.ServiceId, item.Quantity + 1);
    }

    [RelayCommand]
    private void DecrementQuantity(CartItem? item)
    {
        if (item is null) return;
        _cartService.UpdateQuantity(item.GarmentId, item.ServiceId, item.Quantity - 1);
    }

    [RelayCommand]
    private void RemoveItem(CartItem? item)
    {
        if (item is null) return;
        _cartService.Remove(item.GarmentId, item.ServiceId);
    }

    [RelayCommand]
    private void ToggleAddAddressForm() => ShowAddAddressForm = !ShowAddAddressForm;

    [RelayCommand]
    private async Task AddAddressAsync()
    {
        if (string.IsNullOrWhiteSpace(NewAddressLabel) || string.IsNullOrWhiteSpace(NewAddressLine1) ||
            string.IsNullOrWhiteSpace(NewAddressCity) || string.IsNullOrWhiteSpace(NewAddressState) ||
            string.IsNullOrWhiteSpace(NewAddressPostalCode))
        {
            ErrorMessage = "Please fill in all required address fields.";
            return;
        }

        try
        {
            var isFirst = Addresses.Count == 0;
            var response = await _apiClient.AddMyAddressAsync(new CreateCustomerAddressRequest(
                NewAddressLabel, NewAddressLine1, NewAddressLine2, NewAddressCity, NewAddressState, NewAddressPostalCode, isFirst));

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Couldn't save the address.";
                return;
            }

            NewAddressLabel = string.Empty;
            NewAddressLine1 = string.Empty;
            NewAddressLine2 = null;
            NewAddressCity = string.Empty;
            NewAddressState = string.Empty;
            NewAddressPostalCode = string.Empty;
            ShowAddAddressForm = false;

            await ReloadAddressesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't save the address: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ApplyPromo()
    {
        var code = PromoCodeText.Trim();
        if (string.IsNullOrEmpty(code))
        {
            AppliedPromo = null;
            PromoMessage = null;
            RaiseTotalsChanged();
            return;
        }

        var match = Promotions.FirstOrDefault(p => string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            AppliedPromo = null;
            PromoMessage = "Invalid or expired promo code.";
        }
        else
        {
            AppliedPromo = match;
            PromoMessage = $"Applied: {match.Title}";
        }

        RaiseTotalsChanged();
    }

    [RelayCommand]
    private void RemovePromo()
    {
        AppliedPromo = null;
        PromoCodeText = string.Empty;
        PromoMessage = null;
        RaiseTotalsChanged();
    }

    private decimal ComputeDiscount()
    {
        if (AppliedPromo is null) return 0;
        var subtotal = Subtotal;
        var discount = AppliedPromo.DiscountPercent is decimal pct
            ? subtotal * pct / 100m
            : AppliedPromo.DiscountAmount ?? 0;
        return Math.Min(discount, subtotal);
    }

    [RelayCommand]
    private void SelectPickupSlot(int hour) => PickupSlotHour = hour;

    [RelayCommand]
    private void SelectDeliverySlot(int hour) => DeliverySlotHour = hour;

    private static DateTimeOffset? BuildScheduled(DateTime date, int? hour) =>
        hour is int h ? new DateTimeOffset(date.Date.AddHours(h), DateTimeOffset.Now.Offset) : null;

    [RelayCommand]
    private async Task PlaceOrderAsync()
    {
        if (Items.Count == 0)
        {
            ErrorMessage = "Your cart is empty.";
            return;
        }

        IsPlacingOrder = true;
        ErrorMessage = null;

        try
        {
            var profile = await _apiClient.GetMyCustomerProfileAsync();
            if (profile is null)
            {
                ErrorMessage = "Couldn't load your customer profile.";
                return;
            }

            var request = new CreateOrderRequest(
                profile.Id,
                IsExpress ? OrderChannel.Express : OrderChannel.PickupRequest,
                IsExpress,
                Items.Select(i => new CreateOrderItemRequest(i.GarmentId, i.ServiceId, i.Quantity, i.WeightKg, null)).ToList(),
                BuildScheduled(PickupDate, PickupSlotHour),
                SelectedAddress?.Id,
                AppliedPromo?.Code,
                BuildScheduled(DeliveryDate, DeliverySlotHour));

            var response = await _apiClient.CreateOrderAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = await ExtractErrorMessageAsync(response);
                return;
            }

            _cartService.Clear();
            AppliedPromo = null;
            PromoCodeText = string.Empty;
            await Shell.Current.GoToAsync("//MyRequestsPage");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't place your order: {ex.Message}";
        }
        finally
        {
            IsPlacingOrder = false;
        }
    }

    // The API's ExceptionHandlingMiddleware returns { title, errors? } — surfacing that
    // instead of a generic message is the difference between "couldn't place order" and
    // actually telling the customer why (e.g. "Promo code 'X' is invalid or has expired.").
    private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
            return $"Couldn't place your order (status {(int)response.StatusCode}). Please try again.";

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
            {
                var title = titleProp.GetString();
                if (doc.RootElement.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Array
                    && errorsProp.GetArrayLength() > 0)
                {
                    var firstError = errorsProp[0];
                    if (firstError.TryGetProperty("errorMessage", out var msgProp))
                        return $"{title}: {msgProp.GetString()}";
                }
                return title ?? body;
            }
        }
        catch (JsonException)
        {
            // Not JSON (e.g. an ASP.NET default error page) — fall through to the raw body.
        }

        return body;
    }
}
