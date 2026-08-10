using System.Globalization;

namespace LaundryMgmt.Mobile.Converters;

/// <summary>Image URLs are baked in at upload time as
/// "{Request.Scheme}://{Request.Host}/uploads/{file}" (see UploadsController) — whatever
/// host issued the upload request. That's fine for the web app (same-origin as the
/// browser), but on a physical device an image uploaded from the admin's desktop browser
/// comes back as "https://localhost:5101/..." which means the phone itself, not the dev
/// PC (same class of bug as MauiProgram.ApiBaseUrl). This rewrites the scheme/host/port
/// to whatever the running app is currently using to reach the API, keeping only the
/// path, so the same file always resolves correctly regardless of which host originally
/// uploaded it.</summary>
public class ResolvedImageUrlConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string url || string.IsNullOrWhiteSpace(url))
            return null;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url;

        var apiBase = new Uri(MauiProgram.ApiBaseUrl);
        var rewritten = new UriBuilder(apiBase) { Path = uri.AbsolutePath, Query = uri.Query.TrimStart('?') };
        return rewritten.Uri.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
