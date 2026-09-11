using System.Globalization;

namespace LaundryMgmt.Mobile.Converters;

/// <summary>Legacy local-disk image URLs were baked in at upload time as
/// "{Request.Scheme}://{Request.Host}/uploads/{file}" (see UploadsController's old
/// implementation) — whatever host issued the upload request. That's fine for the web
/// app (same-origin as the browser), but on a physical device an image uploaded from
/// the admin's desktop browser came back as "https://localhost:5101/..." which means
/// the phone itself, not the dev PC (same class of bug as MauiProgram.ApiBaseUrl). For
/// any URL still shaped like that (old data), this rewrites the scheme/host/port to
/// whatever the app is currently using to reach the API.
/// Uploads now go to Azure Blob Storage instead, into a container that is ALSO named
/// "uploads" — so a blob URL's path is "/uploads/{file}" too, the exact same shape as
/// the legacy local-disk path. Do not key off the path to decide what to rewrite: that
/// would (and did) mistake fresh, correct blob URLs for legacy ones and mangle them
/// onto the API host, where the file doesn't exist. Key off host instead — anything
/// already on *.blob.core.windows.net is correct as issued and must be left untouched.</summary>
public class ResolvedImageUrlConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string url || string.IsNullOrWhiteSpace(url))
            return null;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url;

        if (uri.Host.EndsWith(".blob.core.windows.net", StringComparison.OrdinalIgnoreCase))
            return url;

        var apiBase = new Uri(MauiProgram.ApiBaseUrl);
        var rewritten = new UriBuilder(apiBase) { Path = uri.AbsolutePath, Query = uri.Query.TrimStart('?') };
        return rewritten.Uri.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
