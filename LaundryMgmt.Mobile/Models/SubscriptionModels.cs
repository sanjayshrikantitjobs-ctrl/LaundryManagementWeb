namespace LaundryMgmt.Mobile.Models;

public enum BillingCycle
{
    Monthly = 0,
    Quarterly = 1,
    Yearly = 2,
    HalfYearly = 3
}

public record SubscriptionPlanFeatureDto(Guid Id, string Text, int DisplayOrder);

public record SubscriptionPlanDto(
    Guid Id, string Name, string? Description, BillingCycle BillingCycle, int GarmentsPerCycle,
    decimal Price, int DisplayOrder, bool IsActive, List<SubscriptionPlanFeatureDto> Features);

public record CreateSubscriptionPlanRequest(
    string Name, string? Description, BillingCycle BillingCycle, int GarmentsPerCycle,
    decimal Price, int DisplayOrder, List<string> Features);

public record UpdateSubscriptionPlanRequest(
    string Name, string? Description, BillingCycle BillingCycle, int GarmentsPerCycle,
    decimal Price, int DisplayOrder, bool IsActive, List<string> Features);

public enum SubscriptionStatus
{
    Active = 0,
    Paused = 1,
    Cancelled = 2,
    Expired = 3
}

public record CustomerSubscriptionListItemDto(
    Guid Id, Guid CustomerId, string CustomerName, Guid SubscriptionPlanId, string PlanName,
    decimal RecurringValue, DateOnly StartDate, DateOnly EndDate, DateOnly? NextBillingDate,
    SubscriptionStatus Status, string? Notes);
