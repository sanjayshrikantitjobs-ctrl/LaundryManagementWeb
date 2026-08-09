using LaundryMgmt.Domain.Common;
using LaundryMgmt.Domain.Enums;

namespace LaundryMgmt.Domain.Entities;

public class Garment : AuditableEntity
{
    public string Name { get; set; } = string.Empty;        // Shirt, Pant, Saree...
    public string Category { get; set; } = string.Empty;    // Menswear, Womenswear, Home...
    public string? Barcode { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? ImageUrl { get; set; }

    public ICollection<GarmentServicePrice> ServicePrices { get; set; } = new List<GarmentServicePrice>();
}

public class Service : AuditableEntity
{
    public string Name { get; set; } = string.Empty;        // Washing, Dry Cleaning, Iron Only...
    public decimal BasePrice { get; set; }
    public int EstimatedTimeHours { get; set; }
    public decimal GstPercentage { get; set; }
    public int Priority { get; set; }
    public string? ImageUrl { get; set; }

    /// <summary>Extra flat charge added per item when a customer picks the Express
    /// channel for an order containing this service (admin-configurable).</summary>
    public decimal ExpressSurcharge { get; set; }

    /// <summary>Turnaround time (hours) promised when Express is selected — distinct
    /// from EstimatedTimeHours, which is the normal-channel turnaround.</summary>
    public int ExpressEtaHours { get; set; }

    public ICollection<GarmentServicePrice> GarmentPrices { get; set; } = new List<GarmentServicePrice>();
}

/// <summary>Price for a specific Garment + Service combination (e.g. Shirt + Wash&Iron = 40).</summary>
public class GarmentServicePrice : BaseEntity
{
    public Guid GarmentId { get; set; }
    public Garment? Garment { get; set; }

    public Guid ServiceId { get; set; }
    public Service? Service { get; set; }

    public PricingType PricingType { get; set; } = PricingType.PerItem;
    public decimal Price { get; set; }          // per item or per kg depending on PricingType
}
