namespace LaundryMgmt.Mobile.Models;

public record ActivePromotionDto(
    Guid Id, string Title, string? Description, string? ImageUrl, string? Code,
    decimal? DiscountPercent, decimal? DiscountAmount, DateTimeOffset? ValidTo);
