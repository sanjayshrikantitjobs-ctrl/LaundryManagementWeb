using LaundryMgmt.Domain.Common;

namespace LaundryMgmt.Domain.Entities;

/// <summary>An installed app instance's FCM registration token, used to send it OS-level
/// push notifications. Keyed off the Identity login (UserId), the same convention
/// <see cref="Notification.RecipientUserId"/> and <see cref="Customer.IdentityUserId"/>
/// already use — so this can address admin devices too, not just customers, without a
/// schema change. One row per device; re-registering the same token just refreshes it
/// (see RegisterDeviceTokenCommand's upsert-by-token logic).</summary>
public class DeviceToken : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;

    /// <summary>Plain string, not an enum — "Android" today, but adding iOS later
    /// needs no schema change.</summary>
    public string Platform { get; set; } = string.Empty;

    public DateTimeOffset LastSeenUtc { get; set; }
}
