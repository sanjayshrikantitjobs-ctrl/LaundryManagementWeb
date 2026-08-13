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

public record PaginatedList<T>(
    List<T> Items, int PageNumber, int PageSize, int TotalPages, int TotalCount, bool HasNextPage, bool HasPreviousPage);

public enum CustomerStatus
{
    Active = 0,
    Inactive = 1
}

public record CustomerListItem(
    Guid Id, string FullName, string PhoneNumber, string? Email, string? WhatsAppNumber,
    MembershipTier MembershipTier, decimal WalletBalance, int LoyaltyPoints, CustomerStatus Status);

public record CreateCustomerRequest(string FullName, string PhoneNumber, string? Email, decimal CreditLimit, string? Notes);

public record UpdateCustomerRequest(
    string FullName, string PhoneNumber, string? Email, decimal CreditLimit, MembershipTier MembershipTier, string? Notes);

public record UpdateMyProfileRequest(string FullName, string? Email, string? WhatsAppNumber);

public record CustomerAddressDto(
    Guid Id, string Label, string Line1, string? Line2, string City, string State, string PostalCode, bool IsDefault);

public record CreateCustomerAddressRequest(
    string Label, string Line1, string? Line2, string City, string State, string PostalCode, bool IsDefault);

public record CustomerDetailDto(
    Guid Id, string FullName, string PhoneNumber, string? Email,
    decimal WalletBalance, decimal CreditLimit, int LoyaltyPoints,
    MembershipTier MembershipTier, CustomerStatus Status, string? Notes, List<CustomerAddressDto> Addresses);

public record ServiceCategoryDto(Guid Id, string Name, string? Description, string? IconOrImageUrl, int DisplayOrder, bool IsActive);

public record GarmentListItem(Guid Id, string Name, Guid CategoryId, string CategoryName, string? ImageUrl);

public record CreateGarmentRequest(string Name, Guid CategoryId, string? SpecialInstructions);

public record UpdateGarmentRequest(string Name, Guid CategoryId, string? SpecialInstructions, string? ImageUrl);

public record ServiceListItem(
    Guid Id, string Name, decimal BasePrice, int EstimatedTimeHours, decimal GstPercentage, int Priority,
    string? ImageUrl, decimal ExpressSurcharge, int ExpressEtaHours);

public record ServiceDetailDto(
    Guid Id, string Name, Guid CategoryId, string CategoryName, decimal BasePrice, int EstimatedTimeHours,
    decimal GstPercentage, int Priority, string? ImageUrl, decimal ExpressSurcharge, int ExpressEtaHours);

public record CreateServiceRequest(
    string Name, Guid CategoryId, decimal BasePrice, int EstimatedTimeHours, decimal GstPercentage, int Priority,
    string? ImageUrl = null, decimal ExpressSurcharge = 0, int ExpressEtaHours = 24);

public record UpdateServiceRequest(
    string Name, Guid CategoryId, decimal BasePrice, int EstimatedTimeHours, decimal GstPercentage, int Priority,
    string? ImageUrl, decimal ExpressSurcharge, int ExpressEtaHours);

public record GarmentServicePriceDto(Guid ServiceId, string ServiceName, PricingType PricingType, decimal Price);

public record GarmentDetailDto(
    Guid Id, string Name, Guid CategoryId, string CategoryName, string? SpecialInstructions, string? ImageUrl,
    List<GarmentServicePriceDto> ServicePrices);

public record PricingMatrixServiceDto(Guid Id, string Name, Guid CategoryId, string CategoryName);

public record PricingMatrixCellDto(Guid ServiceId, PricingType? PricingType, decimal? Price);

public record PricingMatrixGarmentRowDto(Guid GarmentId, string GarmentName, Guid CategoryId, string CategoryName, List<PricingMatrixCellDto> Prices);

public record PricingMatrixDto(List<PricingMatrixServiceDto> Services, List<PricingMatrixGarmentRowDto> Garments);
