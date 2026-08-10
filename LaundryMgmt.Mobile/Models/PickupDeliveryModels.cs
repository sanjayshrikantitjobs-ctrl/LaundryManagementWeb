namespace LaundryMgmt.Mobile.Models;

public enum DeliveryStatus
{
    NotScheduled = 0,
    Scheduled = 1,
    OutForPickup = 2,
    PickedUp = 3,
    OutForDelivery = 4,
    Delivered = 5,
    Failed = 6
}

public record MyPickupDeliveryDto(
    Guid PickupDeliveryId, Guid OrderId, string OrderNumber, string CustomerName,
    string? AddressLine, DeliveryStatus Status, DateTimeOffset? ScheduledAtUtc, DateTimeOffset? CompletedAtUtc);

public record OrderPickupDeliveryDto(
    Guid Id, bool IsPickup, DeliveryStatus Status, DateTimeOffset? ScheduledAtUtc, DateTimeOffset? CompletedAtUtc,
    Guid? AssignedEmployeeId, string? AssignedEmployeeName);

public record AgentDto(Guid EmployeeId, string FullName);

public record AssignAgentRequest(Guid EmployeeId);
