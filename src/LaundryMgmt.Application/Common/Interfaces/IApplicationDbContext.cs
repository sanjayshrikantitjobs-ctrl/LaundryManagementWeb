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
    DbSet<ServiceCategory> ServiceCategories { get; }
    DbSet<Garment> Garments { get; }
    DbSet<Service> Services { get; }
    DbSet<GarmentServicePrice> GarmentServicePrices { get; }
    DbSet<AddOn> AddOns { get; }
    DbSet<OrderItemAddOn> OrderItemAddOns { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Payment> Payments { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<Employee> Employees { get; }
    DbSet<Machine> Machines { get; }
    DbSet<Complaint> Complaints { get; }
    DbSet<PickupDelivery> PickupDeliveries { get; }
    DbSet<OrderGarmentImage> OrderGarmentImages { get; }
    DbSet<OtpChallenge> OtpChallenges { get; }
    DbSet<Promotion> Promotions { get; }
    DbSet<ContactMessage> ContactMessages { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<SubscriptionPlanFeature> SubscriptionPlanFeatures { get; }
    DbSet<CustomerSubscription> CustomerSubscriptions { get; }
    DbSet<DeviceToken> DeviceTokens { get; }

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

/// <summary>OS-level push notifications (Android via Firebase Cloud Messaging).
/// Implemented in Infrastructure by wrapping the FirebaseAdmin SDK. Distinct from
/// <see cref="INotificationService"/> (SMS/WhatsApp, currently unimplemented) and from
/// the in-app <see cref="Notification"/> feed — this is the only one of the three that
/// reaches a customer while the app isn't open.</summary>
public interface IPushNotificationService
{
    Task SendToUserAsync(Guid userId, string title, string body, string? entityId, NotificationType type, CancellationToken cancellationToken = default);
    Task SendToAllCustomersAsync(string title, string body, string? entityId, NotificationType type, CancellationToken cancellationToken = default);
}

/// <summary>Stores uploaded catalog/promotion images (garment/service/category photos).
/// Implemented in Infrastructure via Azure Blob Storage — deliberately NOT local disk,
/// which doesn't survive a redeploy or scale across instances (see UploadsController's
/// prior implementation, replaced by this).</summary>
public interface IImageStorageService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
}
