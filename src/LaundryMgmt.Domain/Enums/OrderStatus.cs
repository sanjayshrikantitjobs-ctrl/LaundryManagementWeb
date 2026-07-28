namespace LaundryMgmt.Domain.Enums;

/// <summary>
/// Linear order workflow: New -> Received -> Sorting -> Washing -> Drying
/// -> Ironing -> Packing -> ReadyForDelivery -> Delivered.
/// Cancelled can happen from any pre-Delivered state.
/// </summary>
public enum OrderStatus
{
    New = 0,
    Received = 1,
    Sorting = 2,
    Washing = 3,
    Drying = 4,
    Ironing = 5,
    Packing = 6,
    ReadyForDelivery = 7,
    Delivered = 8,
    Cancelled = 9
}
