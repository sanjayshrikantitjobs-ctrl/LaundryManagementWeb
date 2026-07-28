using LaundryMgmt.Domain.Common;
using LaundryMgmt.Domain.Enums;

namespace LaundryMgmt.Domain.Entities;

public class Customer : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }

    public decimal WalletBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public int LoyaltyPoints { get; set; }
    public MembershipTier MembershipTier { get; set; } = MembershipTier.None;
    public string? Notes { get; set; }

    public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

public class CustomerAddress : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string Label { get; set; } = "Home"; // Home, Work, Other
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
