using System.Globalization;
using LaundryMgmt.Mobile.Models;

namespace LaundryMgmt.Mobile.Converters;

/// <summary>Badge color for a CustomerStatus value — green for Active, gray for Inactive.</summary>
public class CustomerStatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is CustomerStatus.Inactive ? Colors.Gray : Color.FromArgb("#15803D");

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Toggle-button label for a CustomerStatus value — the action it would take, not the current state.</summary>
public class CustomerStatusToggleLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is CustomerStatus.Inactive ? "Activate" : "Deactivate";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Same idea for a plain bool IsActive flag (e.g. UserSummaryDto) — the
/// action the toggle button would take, not the current state.</summary>
public class ActiveToggleLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? "Deactivate" : "Activate";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
