using LaundryMgmt.Application.Common.Interfaces;
using LaundryMgmt.Domain.Entities;
using LaundryMgmt.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaundryMgmt.Infrastructure.EventHandlers;

/// <summary>Sends the customer a push when their order's status changes. Lives in
/// Infrastructure (unlike OrderStatusChangedSignalRHandler, which needs the API-layer
/// IHubContext) because it only needs IApplicationDbContext + IPushNotificationService.
/// OrderStatusChangedEvent is raised from both Order.AdvanceTo and Order.SetStatus, so
/// this fires for both the pipeline "advance" action and the general admin edit form
/// without either command handler needing to know about push at all.</summary>
public class OrderStatusChangedPushHandler : INotificationHandler<OrderStatusChangedEvent>
{
    private readonly IApplicationDbContext _db;
    private readonly IPushNotificationService _pushService;

    public OrderStatusChangedPushHandler(IApplicationDbContext db, IPushNotificationService pushService)
    {
        _db = db;
        _pushService = pushService;
    }

    public async Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == notification.OrderId, cancellationToken);

        if (order?.Customer?.IdentityUserId is not { } userId) return;

        await _pushService.SendToUserAsync(
            userId,
            "Order update",
            $"Your order {notification.OrderNumber} is now {notification.NewStatus}.",
            notification.OrderId.ToString(),
            NotificationType.OrderUpdated,
            cancellationToken);
    }
}
