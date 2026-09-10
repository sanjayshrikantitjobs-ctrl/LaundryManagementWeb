using LaundryMgmt.Application.Common.Interfaces;
using LaundryMgmt.Domain.Entities;
using LaundryMgmt.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaundryMgmt.Infrastructure.EventHandlers;

/// <summary>Writes one in-app Notification row per customer with a linked login, so
/// the bell/notifications feed shows the new promotion the same way it shows an order
/// update — the existing Notification model is one-row-per-recipient, so a broadcast
/// is just that fan-out done in bulk, not a new concept.</summary>
public class PromotionCreatedNotificationHandler : INotificationHandler<PromotionCreatedEvent>
{
    private readonly IApplicationDbContext _db;

    public PromotionCreatedNotificationHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(PromotionCreatedEvent notification, CancellationToken cancellationToken)
    {
        var customerUserIds = await _db.Customers
            .Where(c => c.IdentityUserId != null)
            .Select(c => c.IdentityUserId!.Value)
            .ToListAsync(cancellationToken);

        if (customerUserIds.Count == 0) return;

        foreach (var userId in customerUserIds)
        {
            _db.Notifications.Add(new Notification
            {
                RecipientUserId = userId,
                Type = NotificationType.PromotionCreated,
                Title = "New offer",
                Message = $"Check out our new promotion: {notification.Title}.",
                EntityId = notification.PromotionId.ToString()
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
