using LaundryMgmt.Mobile.Models;

namespace LaundryMgmt.Mobile.Services;

/// <summary>Where a notification (in-app tap or OS push tap) should navigate to.
/// Shared by NotificationsViewModel.OpenAsync (in-app list) and the push tap handler
/// (App.xaml.cs) so both routes identically off the same Type/EntityId pair.</summary>
public static class NotificationRouting
{
    public static string? BuildRoute(NotificationType type, string? entityId)
    {
        // Fixed destination — a promotion doesn't have its own detail page yet, just
        // the Promotions tab. "//" switches tabs rather than pushing onto the stack.
        if (type == NotificationType.PromotionCreated)
            return "//PromotionsPage";

        if (string.IsNullOrEmpty(entityId)) return null;

        return type == NotificationType.NewCustomerRegistered
            ? $"{nameof(Views.CustomerDetailPage)}?customerId={entityId}"
            : $"{nameof(Views.OrderDetailPage)}?orderId={entityId}";
    }
}
