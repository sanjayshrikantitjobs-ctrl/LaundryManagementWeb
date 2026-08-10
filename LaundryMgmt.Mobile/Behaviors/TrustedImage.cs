using LaundryMgmt.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LaundryMgmt.Mobile.Behaviors;

/// <summary>Attached property for &lt;Image&gt; that downloads the bytes through the app's
/// singleton ApiClient (whose HttpClient trusts the DEBUG-only self-signed dev cert) instead
/// of letting Image.Source load the URL directly through MAUI's own networking, which does
/// not share that trust and silently fails on Android/iOS. Usage:
/// &lt;Image behaviors:TrustedImage.Url="{Binding ImageUrl, Converter={StaticResource ResolvedImageUrlConverter}}" /&gt;
/// — do not also set Source; this property sets it programmatically once the download completes.</summary>
public static class TrustedImage
{
    public static readonly BindableProperty UrlProperty = BindableProperty.CreateAttached(
        "Url", typeof(string), typeof(TrustedImage), null, propertyChanged: OnUrlChanged);

    public static string? GetUrl(BindableObject view) => (string?)view.GetValue(UrlProperty);
    public static void SetUrl(BindableObject view, string? value) => view.SetValue(UrlProperty, value);

    private static async void OnUrlChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not Image image) return;

        var url = newValue as string;
        if (string.IsNullOrWhiteSpace(url))
        {
            image.Source = null;
            return;
        }

        var apiClient = IPlatformApplication.Current?.Services.GetService<ApiClient>();
        if (apiClient is null) return;

        var bytes = await apiClient.DownloadImageBytesAsync(url);
        if (bytes is null) return;

        // The Url could have changed again while the download was in flight — only
        // apply the result if this is still the most recently requested URL.
        if (GetUrl(image) == url)
            image.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
    }
}
