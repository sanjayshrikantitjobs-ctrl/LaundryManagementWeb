using LaundryMgmt.Domain.Common;

namespace LaundryMgmt.Domain.Events;

public sealed class PromotionCreatedEvent : IDomainEvent
{
    public Guid PromotionId { get; }
    public string Title { get; }
    public DateTimeOffset OccurredOnUtc { get; }

    public PromotionCreatedEvent(Guid promotionId, string title)
    {
        PromotionId = promotionId;
        Title = title;
        OccurredOnUtc = DateTimeOffset.UtcNow;
    }
}
