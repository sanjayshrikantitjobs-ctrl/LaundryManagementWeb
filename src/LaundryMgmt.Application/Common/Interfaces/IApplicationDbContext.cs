using LaundryMgmt.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaundryMgmt.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext so Application-layer handlers
/// depend only on this interface, never on Infrastructure/EF Core directly.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<CustomerAddress> CustomerAddresses { get; }
    DbSet<Garment> Garments { get; }
    DbSet<Service> Services { get; }
    DbSet<GarmentServicePrice> GarmentServicePrices { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Payment> Payments { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<Employee> Employees { get; }
    DbSet<Machine> Machines { get; }
    DbSet<Complaint> Complaints { get; }
    DbSet<PickupDelivery> PickupDeliveries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? Role { get; }
}

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}

/// <summary>Sends notifications (SMS/Email/WhatsApp). Implemented in Infrastructure
/// via Twilio/SendGrid; queued through Hangfire for reliability.</summary>
public interface INotificationService
{
    Task SendOrderStatusNotificationAsync(Guid orderId, CancellationToken cancellationToken = default);
}
