using LaundryMgmt.Application.Common.Interfaces;
using LaundryMgmt.Domain.Entities;
using LaundryMgmt.Domain.Events;
using MediatR;

namespace LaundryMgmt.Infrastructure.EventHandlers;

/// <summary>Pushes the new promotion to every customer device — a separate handler
/// from PromotionCreatedNotificationHandler (in-app feed) so each stays single-purpose,
/// same as how OrderStatusChangedEvent has one handler per concern (SignalR, push).</summary>
public class PromotionCreatedPushHandler : INotificationHandler<PromotionCreatedEvent>
{
    private readonly IPushNotificationService _pushService;

    public PromotionCreatedPushHandler(IPushNotificationService pushService) => _pushService = pushService;

    public Task Handle(PromotionCreatedEvent notification, CancellationToken cancellationToken) =>
        _pushService.SendToAllCustomersAsync(
            "New offer",
            $"Check out our new promotion: {notification.Title}.",
            notification.PromotionId.ToString(),
            NotificationType.PromotionCreated,
            cancellationToken);
}
