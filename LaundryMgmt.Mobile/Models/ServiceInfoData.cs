namespace LaundryMgmt.Mobile.Models;

public record ServiceComparisonRow(string Feature, bool Ours, bool Others)
{
    public string OursIcon => Ours ? "✅" : "❌";
    public string OthersIcon => Others ? "✅" : "❌";
}

public record ServiceInfoCategory(
    string Slug, string Name, string Icon, string Headline, string Description,
    List<ServiceComparisonRow> Comparison, List<string> Benefits);

/// <summary>Hand-ported from client-web/src/app/features/service-info/service-info.data.ts —
/// genuinely static marketing copy, no API call backs this page on either platform.</summary>
public static class ServiceInfoCategories
{
    public static readonly List<ServiceInfoCategory> All = new()
    {
        new ServiceInfoCategory(
            "laundry", "Laundry", "🧺",
            "Reliable everyday laundry, done right every time",
            "Our standard wash-and-fold process is built around consistency: the right water temperature, the right detergent dosage, and careful sorting by fabric type — so your everyday clothes come back clean, fresh, and exactly the way you handed them in.",
            new List<ServiceComparisonRow>
            {
                new("Fabric-based sorting before wash", true, false),
                new("Stain pre-treatment included", true, false),
                new("Fragrance-free option available", true, false),
                new("Folded & packed for pickup", true, true)
            },
            new List<string>
            {
                "Separate wash cycles for whites, colors, and delicates",
                "No harsh bleach — fabric-safe detergents only",
                "Every load checked for pockets and loose items before washing"
            }),
        new ServiceInfoCategory(
            "dry-cleaning", "Dry Cleaning", "👔",
            "Impeccable dry cleaning quality, every time",
            "Our eco-friendly dry cleaning process is built to protect delicate fabrics, structured garments, and anything with a \"dry clean only\" label — restoring shine and texture without the shrinkage or color fade you get from harsher solvent-based cleaners.",
            new List<ServiceComparisonRow>
            {
                new("100% garment care label adherence", true, false),
                new("Color-bleed proof process", true, false),
                new("Zero shrinkage guaranteed", true, false),
                new("99% stain removal promise", true, false)
            },
            new List<string>
            {
                "Zero cloth shrinkage, 100% preservation of shine and texture",
                "Zero harm to the environment — no toxic oil-based solvents",
                "Specialized treatment for tough India-specific stains (turmeric, henna, ink)"
            }),
        new ServiceInfoCategory(
            "shoe-cleaning", "Shoe Cleaning", "👟",
            "Bring your sneakers and formal shoes back to life",
            "From canvas sneakers to leather formals, our shoe cleaning service handles sole scrubbing, upper cleaning, deodorizing, and conditioning — with materials matched to the shoe so nothing gets damaged in the process.",
            new List<ServiceComparisonRow>
            {
                new("Material-matched cleaning agents", true, false),
                new("Deep sole & midsole scrub", true, true),
                new("Deodorizing treatment included", true, false),
                new("Suede & leather conditioning", true, false)
            },
            new List<string>
            {
                "Safe for canvas, leather, suede, and sports mesh",
                "Odor-neutralizing treatment, not just masking fragrance",
                "Laces cleaned separately to avoid dye transfer"
            }),
        new ServiceInfoCategory(
            "carpet-cleaning", "Carpet Cleaning", "🧶",
            "Deep-clean carpets without soaking the backing",
            "Carpets trap dust, allergens, and stains deep in the pile. Our process combines pre-vacuuming, targeted stain treatment, and controlled-moisture washing so the carpet backing stays intact and dries faster.",
            new List<ServiceComparisonRow>
            {
                new("Pre-vacuum before wet cleaning", true, false),
                new("Controlled moisture (backing-safe)", true, false),
                new("Allergen & dust-mite treatment", true, false),
                new("Pickup & delivery for large carpets", true, true)
            },
            new List<string>
            {
                "Safe for wool, silk-blend, and synthetic carpets",
                "Faster drying thanks to controlled water use",
                "Free pickup for large or heavy carpets"
            }),
        new ServiceInfoCategory(
            "leather-cleaning", "Leather Cleaning", "🧥",
            "Specialized care for leather jackets, bags, and more",
            "Leather needs conditioning, not just cleaning. Our leather care service cleans surface dirt and stains, then reconditions the material to prevent cracking — keeping jackets, bags, and accessories supple for longer.",
            new List<ServiceComparisonRow>
            {
                new("pH-balanced leather cleaners", true, false),
                new("Post-clean conditioning included", true, false),
                new("Color touch-up for scuffed edges", true, false),
                new("Safe for genuine & faux leather", true, true)
            },
            new List<string>
            {
                "Prevents cracking and drying with proper reconditioning",
                "Gentle on stitching and hardware (zips, studs, buckles)",
                "Suitable for jackets, bags, wallets, and upholstery"
            })
    };
}
