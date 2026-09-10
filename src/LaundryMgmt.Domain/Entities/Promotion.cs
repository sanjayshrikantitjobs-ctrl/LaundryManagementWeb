using LaundryMgmt.Domain.Common;
using LaundryMgmt.Domain.Events;

namespace LaundryMgmt.Domain.Entities;

/// <summary>Admin-managed promotion/offer shown to customers (with an image) on the
/// Promotions & Offers page, optionally redeemable via a promo code at checkout.</summary>
public class Promotion : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Code { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Raises <see cref="PromotionCreatedEvent"/> — called once, right after
    /// the entity is added, by CreatePromotionCommandHandler. A method rather than a
    /// constructor because the handler still builds this via an object initializer
    /// (matches the existing style here); mirrors how Order.AdvanceTo/SetStatus keep
    /// event-raising inside the entity rather than the command handler.</summary>
    public void MarkCreated() => AddDomainEvent(new PromotionCreatedEvent(Id, Title));
}
