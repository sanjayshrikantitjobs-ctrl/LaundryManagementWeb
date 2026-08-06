namespace LaundryMgmt.Mobile.Models;

public enum PricingType
{
    PerItem = 0,
    WeightBased = 1
}

public enum MembershipTier
{
    None = 0,
    Silver = 1,
    Gold = 2
}

public record PaginatedList<T>(List<T> Items, int PageNumber, int TotalPages, int TotalCount);

public record CustomerListItem(
    Guid Id, string FullName, string PhoneNumber, string? Email,
    MembershipTier MembershipTier, decimal WalletBalance, int LoyaltyPoints);

public record CreateCustomerRequest(string FullName, string PhoneNumber, string? Email, decimal CreditLimit, string? Notes);

public record GarmentListItem(Guid Id, string Name, string Category, string? Barcode);

public record CreateGarmentRequest(string Name, string Category, string? Barcode, string? SpecialInstructions);

public record ServiceListItem(
    Guid Id, string Name, decimal BasePrice, int EstimatedTimeHours, decimal GstPercentage, int Priority);

public record CreateServiceRequest(string Name, decimal BasePrice, int EstimatedTimeHours, decimal GstPercentage, int Priority);

public record GarmentServicePriceDto(Guid ServiceId, string ServiceName, PricingType PricingType, decimal Price);

public record GarmentDetailDto(
    Guid Id, string Name, string Category, string? Barcode, string? SpecialInstructions,
    List<GarmentServicePriceDto> ServicePrices);

public record PricingMatrixServiceDto(Guid Id, string Name);

public record PricingMatrixCellDto(Guid ServiceId, PricingType? PricingType, decimal? Price);

public record PricingMatrixGarmentRowDto(Guid GarmentId, string GarmentName, string Category, List<PricingMatrixCellDto> Prices);

public record PricingMatrixDto(List<PricingMatrixServiceDto> Services, List<PricingMatrixGarmentRowDto> Garments);
