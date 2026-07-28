using LaundryMgmt.Domain.Common;
using LaundryMgmt.Domain.Enums;

namespace LaundryMgmt.Domain.Entities;

public class Invoice : AuditableEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty; // e.g. INV-2026-000123
    public decimal SubTotal { get; set; }
    public decimal GstAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTimeOffset IssuedAtUtc { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class Payment : AuditableEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public decimal Amount { get; set; }
    public PaymentMode Mode { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionReference { get; set; }
    public DateTimeOffset PaidAtUtc { get; set; }
}

public class InventoryItem : AuditableEntity
{
    public string Name { get; set; } = string.Empty; // Detergent, Hangers, Packing Covers...
    public string Unit { get; set; } = "pcs";         // pcs, ltr, kg
    public decimal QuantityOnHand { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal UnitCost { get; set; }
}

public class InventoryTransaction : BaseEntity
{
    public Guid InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }

    public bool IsStockIn { get; set; } // true = purchase/stock-in, false = stock-out/usage
    public decimal Quantity { get; set; }
    public string? Reference { get; set; }
    public DateTimeOffset TransactionAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public class Employee : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? Shift { get; set; }
    public decimal Salary { get; set; }
    public DateOnly JoinedOn { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Machine : AuditableEntity
{
    public string Name { get; set; } = string.Empty;   // Washer #1, Dryer #2...
    public string Type { get; set; } = string.Empty;   // Washer, Dryer, Iron
    public decimal CapacityKg { get; set; }
    public MachineStatus Status { get; set; } = MachineStatus.Idle;
    public DateOnly? LastMaintenanceOn { get; set; }
    public DateOnly? NextMaintenanceDue { get; set; }
}

public class Complaint : AuditableEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public ComplaintType Type { get; set; }
    public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;
    public string Description { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public decimal? CompensationAmount { get; set; }
}

public class PickupDelivery : AuditableEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public bool IsPickup { get; set; } // true = pickup, false = delivery
    public Guid? DeliveryBoyEmployeeId { get; set; }
    public Employee? DeliveryBoy { get; set; }

    public DeliveryStatus Status { get; set; } = DeliveryStatus.NotScheduled;
    public DateTimeOffset? ScheduledAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public string? Otp { get; set; }
    public decimal Charge { get; set; }
}
