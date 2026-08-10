namespace LaundryMgmt.Mobile.Models;

public enum GarmentImageType
{
    Pickup = 0,
    Delivery = 1
}

public record OrderGarmentImageDto(
    Guid Id, Guid OrderId, Guid? ServiceId, string? ServiceName, Guid? OrderItemId,
    GarmentImageType ImageType, string ImageUrl, string UploadedByName, DateTimeOffset UploadedAtUtc, string? Notes);

public record AddImageRequest(GarmentImageType ImageType, string ImageUrl, Guid? ServiceId, Guid? OrderItemId, string? Notes);

public record UploadedImageDto(string Url);

public static class ImageLimits
{
    /// <summary>Client-side pre-check only — mirrors API's AppConstants.MaxImagesPerOrderCategory;
    /// the server remains the source of truth and rejects the 11th upload regardless.</summary>
    public const int MaxImagesPerOrderCategory = 10;
}
