namespace LaundryMgmt.Mobile.Models;

public enum OrderStatus
{
    New = 0, Received = 1, Sorting = 2, Washing = 3, Drying = 4,
    Ironing = 5, Packing = 6, ReadyForDelivery = 7, Delivered = 8, Cancelled = 9
}

public enum PaymentStatus
{
    Pending = 0, Partial = 1, Paid = 2, Refunded = 3
}

public enum OrderChannel
{
    WalkIn = 0, PickupRequest = 1, Express = 2
}

public record OrderListItem(
    Guid Id, string OrderNumber, string CustomerName, OrderStatus Status,
    decimal TotalAmount, PaymentStatus PaymentStatus, DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PromisedByUtc);

public record CreateOrderItemRequest(Guid GarmentId, Guid ServiceId, int Quantity, decimal? WeightKg, string? SpecialInstructions);

public record CreateOrderRequest(Guid CustomerId, OrderChannel Channel, bool IsExpress, List<CreateOrderItemRequest> Items);
