namespace LaundryMgmt.Mobile.Models;

public enum UserRole
{
    Admin = 0,
    StoreManager = 1,
    Staff = 2,
    DeliveryAgent = 3,
    Customer = 4,
    DepartmentHead = 5,
    PickupAgent = 6
}

public static class StaffRoles
{
    /// <summary>Roles assignable from the Users screen — Customer accounts are
    /// created via self-registration, not here (matches CreateUserCommandValidator
    /// on the API, which rejects Role == Customer).</summary>
    public static readonly UserRole[] All =
    {
        UserRole.Admin, UserRole.StoreManager, UserRole.Staff,
        UserRole.DepartmentHead, UserRole.PickupAgent, UserRole.DeliveryAgent
    };
}

public record UserSummaryDto(
    Guid Id, string UserName, string? Email, string? PhoneNumber, string FullName, UserRole Role, bool IsActive);

public record CreateUserRequest(
    string FullName, string UserName, string? Email, string? PhoneNumber, string Password, UserRole Role);

public record UpdateUserRequest(string FullName, string? Email, string? PhoneNumber);
