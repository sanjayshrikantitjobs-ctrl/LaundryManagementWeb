using System.Globalization;

namespace LaundryMgmt.Mobile.Converters;

/// <summary>Maps a garment/category name to a representative emoji glyph — a
/// lightweight stand-in for real iconography that needs no image assets or build
/// changes. Swap for actual artwork later if you want a more designed look.</summary>
public class GarmentIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string ?? string.Empty;
        return text.ToLowerInvariant() switch
        {
            var t when t.Contains("shirt") => "👕",
            var t when t.Contains("saree") => "🥻",
            var t when t.Contains("kurta") => "👘",
            var t when t.Contains("pant") || t.Contains("trouser") || t.Contains("jean") => "👖",
            var t when t.Contains("blazer") || t.Contains("jacket") || t.Contains("coat") => "🧥",
            var t when t.Contains("bedsheet") || t.Contains("bed sheet") || t.Contains("linen") => "🛏️",
            var t when t.Contains("blanket") || t.Contains("quilt") => "🧣",
            var t when t.Contains("curtain") => "🪟",
            var t when t.Contains("shoe") => "👟",
            var t when t.Contains("carpet") || t.Contains("rug") => "🟫",
            var t when t.Contains("dress") => "👗",
            _ => "🧺"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Same idea for services (Wash, Iron, Dry Clean, ...).</summary>
public class ServiceIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string ?? string.Empty;
        return text.ToLowerInvariant() switch
        {
            var t when t.Contains("dry clean") => "🧴",
            var t when t.Contains("iron") => "♨️",
            var t when t.Contains("wash") => "🌊",
            var t when t.Contains("premium") => "✨",
            var t when t.Contains("shoe") => "👟",
            var t when t.Contains("carpet") => "🧹",
            var t when t.Contains("stain") => "🧽",
            _ => "🧼"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Same idea for a service category on the Home page — a representative
/// glyph for whichever categories don't have a real photo configured yet.</summary>
public class CategoryIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string ?? string.Empty;
        return text.ToLowerInvariant() switch
        {
            var t when t.Contains("stain") => "💧",
            var t when t.Contains("shoe") || t.Contains("footwear") => "👟",
            var t when t.Contains("bag") || t.Contains("accessor") => "👜",
            var t when t.Contains("bedding") || t.Contains("blanket") || t.Contains("linen") => "🛏️",
            var t when t.Contains("curtain") || t.Contains("furnishing") || t.Contains("home") => "🪟",
            var t when t.Contains("wedding") || t.Contains("traditional") => "👘",
            var t when t.Contains("leather") || t.Contains("suede") => "🧥",
            var t when t.Contains("alteration") || t.Contains("repair") => "✂️",
            var t when t.Contains("commercial") || t.Contains("b2b") => "🏢",
            var t when t.Contains("dry clean") => "🧴",
            var t when t.Contains("iron") || t.Contains("press") => "♨️",
            var t when t.Contains("wash") || t.Contains("laundry") => "🧺",
            _ => "🧺"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
