using System.Globalization;

namespace LaundryMgmt.Mobile.Converters;

public class IsNotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not null && (value is not string s || !string.IsNullOrWhiteSpace(s));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>The inverse of IsNotNullConverter — for a fallback icon/placeholder shown
/// only when a string binding (an image URL, typically) is null/blank. Plugging
/// InvertedBoolConverter onto a string binding doesn't work: it only negates actual
/// bool values, so a non-null string passes through unconverted and MAUI's binding
/// then fails to coerce it to the target bool property, silently leaving IsVisible at
/// its default of true — the fallback would show even when the real image is present.</summary>
public class IsNullOrEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is null || (value is string s && string.IsNullOrWhiteSpace(s));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
