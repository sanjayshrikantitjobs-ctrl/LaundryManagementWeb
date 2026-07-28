using LaundryMgmt.Domain.Common;
using LaundryMgmt.Domain.Enums;
using LaundryMgmt.Domain.Events;
using LaundryMgmt.Domain.Exceptions;

namespace LaundryMgmt.Domain.Entities;

public class Order : AuditableEntity
{
    // Order lifecycle, in the fixed sequence the business defines.
    private static readonly OrderStatus[] Sequence =
    {
        OrderStatus.New, OrderStatus.Received, OrderStatus.Sorting, OrderStatus.Washing,
        OrderStatus.Drying, OrderStatus.Ironing, OrderStatus.Packing,
        OrderStatus.ReadyForDelivery, OrderStatus.Delivered
    };

    public string OrderNumber { get; set; } = string.Empty; // human-friendly, e.g. ORD-2026-000123
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public OrderChannel Channel { get; set; } = OrderChannel.WalkIn;
    public OrderStatus Status { get; private set; } = OrderStatus.New;
    public bool IsExpress { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GstAmount { get; set; }
    public decimal DeliveryCharge { get; set; }
    public decimal TotalAmount { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public decimal AmountPaid { get; set; }

    public DateTimeOffset? PromisedByUtc { get; set; }
    public DateTimeOffset? DeliveredAtUtc { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();

    /// <summary>
    /// Moves the order to the next status in the fixed pipeline, or to
    /// Cancelled from any non-terminal state. Throws if the transition
    /// isn't allowed, so callers (handlers/controllers) don't need to
    /// re-implement the workflow rules.
    /// </summary>
    public void AdvanceTo(OrderStatus newStatus, string? changedBy = null)
    {
        if (Status is OrderStatus.Delivered or OrderStatus.Cancelled)
            throw new DomainException($"Order {OrderNumber} is already in a terminal state ({Status}).");

        if (newStatus == OrderStatus.Cancelled)
        {
            Status = OrderStatus.Cancelled;
        }
        else
        {
            var currentIndex = Array.IndexOf(Sequence, Status);
            var targetIndex = Array.IndexOf(Sequence, newStatus);
            if (targetIndex != currentIndex + 1)
                throw new DomainException(
                    $"Cannot move order {OrderNumber} from {Status} to {newStatus}; only the next step in the pipeline is allowed.");

            Status = newStatus;
            if (newStatus == OrderStatus.Delivered)
                DeliveredAtUtc = DateTimeOffset.UtcNow;
        }

        StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = Id,
            Status = Status,
            ChangedAtUtc = DateTimeOffset.UtcNow,
            ChangedBy = changedBy
        });

        AddDomainEvent(new OrderStatusChangedEvent(Id, OrderNumber, Status));
    }
}

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid GarmentId { get; set; }
    public Garment? Garment { get; set; }

    public Guid ServiceId { get; set; }
    public Service? Service { get; set; }

    public int Quantity { get; set; } = 1;
    public decimal? WeightKg { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public string? Barcode { get; set; }
    public string? SpecialInstructions { get; set; }
}

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
    public string? ChangedBy { get; set; }
    public string? Notes { get; set; }
}
