namespace LaundryMgmt.Mobile.Models;

/// <summary>Mobile-local — no server equivalent. Mirrors client-web's cart.service.ts
/// CartItem shape exactly, including the WeightBased-vs-PerItem line-total distinction.</summary>
public record CartItem(
    Guid GarmentId, string GarmentName, string? GarmentImageUrl, string GarmentCategory,
    Guid ServiceId, string ServiceName, PricingType PricingType, decimal UnitPrice,
    int Quantity, decimal? WeightKg, decimal ExpressSurcharge)
{
    public decimal LineTotal => PricingType == PricingType.WeightBased
        ? UnitPrice * (WeightKg ?? 0)
        : UnitPrice * Quantity;

    public bool IsWeightBased => PricingType == PricingType.WeightBased;
}
