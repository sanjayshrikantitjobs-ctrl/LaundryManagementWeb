namespace LaundryMgmt.Mobile.Models;

public enum NotificationType
{
    NewOrder = 0,
    OrderUpdated = 1,
    NewCustomerRegistered = 2,
    PromotionCreated = 3
}

public record NotificationDto(
    Guid Id, NotificationType Type, string Title, string Message, string? EntityId, bool IsRead, DateTimeOffset CreatedAtUtc);
