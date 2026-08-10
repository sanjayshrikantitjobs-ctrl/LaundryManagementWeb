using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels;
using LaundryMgmt.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace LaundryMgmt.Mobile;

public static class MauiProgram
{
    // The dev machine's LAN IP, for physical-device debugging (see ApiBaseUrl below).
    // Update this if it changes — run `ipconfig` (Windows) / `ifconfig` (Mac) on the
    // dev machine and use the address for whichever network your phone is also on.
    private const string DevMachineLanIp = "192.168.1.106";

    // Talks to the HTTPS endpoint directly (not the HTTP one) so the API's
    // UseHttpsRedirection() middleware never gets a chance to 307-redirect the
    // request — HttpClient follows redirects and, like a browser, drops the
    // Authorization header on a cross-origin redirect, which silently turns every
    // authenticated call into an anonymous one (see client-web/proxy.conf.json for
    // the same issue on the Angular side).
    public static string ApiBaseUrl
    {
        get
        {
#if ANDROID || IOS
            // A physical phone can't reach 10.0.2.2 (an alias that only exists inside
            // the Android emulator) or localhost (which on the phone means the phone
            // itself, not the dev PC) — it needs the dev machine's real LAN IP, and
            // the phone and PC must be on the same Wi-Fi network. The API itself also
            // has to be listening on that network interface, not just loopback — see
            // the "0.0.0.0" applicationUrl in Properties/launchSettings.json.
            if (DeviceInfo.Current.DeviceType == DeviceType.Physical)
                return $"https://{DevMachineLanIp}:5101";
#endif
#if ANDROID
            return "https://10.0.2.2:5101"; // Android emulator's alias for the host machine
#else
            return "https://localhost:5101"; // iOS simulator / Mac Catalyst / Windows: same machine as the API
#endif
        }
    }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddSingleton<ApiClient>(_ => new ApiClient(ApiBaseUrl));
        builder.Services.AddSingleton<AuthTokenStore>();
        builder.Services.AddSingleton<CartService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<DeliveryOrderHubService>();

        builder.Services.AddTransient<AppShell>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();

        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<VerifyOtpViewModel>();
        builder.Services.AddTransient<VerifyOtpPage>();

        builder.Services.AddTransient<OrderQueueViewModel>();
        builder.Services.AddTransient<OrderQueuePage>();

        builder.Services.AddTransient<OrdersViewModel>();
        builder.Services.AddTransient<OrdersPage>();
        builder.Services.AddTransient<OrderFormViewModel>();
        builder.Services.AddTransient<OrderFormPage>();
        builder.Services.AddTransient<OrderDetailViewModel>();
        builder.Services.AddTransient<OrderDetailPage>();

        builder.Services.AddTransient<CustomersViewModel>();
        builder.Services.AddTransient<CustomersPage>();
        builder.Services.AddTransient<CustomerFormViewModel>();
        builder.Services.AddTransient<CustomerFormPage>();

        builder.Services.AddTransient<GarmentsViewModel>();
        builder.Services.AddTransient<GarmentsPage>();
        builder.Services.AddTransient<GarmentFormViewModel>();
        builder.Services.AddTransient<GarmentFormPage>();

        builder.Services.AddTransient<ServicesViewModel>();
        builder.Services.AddTransient<ServicesPage>();
        builder.Services.AddTransient<ServiceFormViewModel>();
        builder.Services.AddTransient<ServiceFormPage>();

        builder.Services.AddTransient<PricingMatrixViewModel>();
        builder.Services.AddTransient<PricingMatrixPage>();

        builder.Services.AddTransient<UsersViewModel>();
        builder.Services.AddTransient<UsersPage>();
        builder.Services.AddTransient<UserFormViewModel>();
        builder.Services.AddTransient<UserFormPage>();

        builder.Services.AddTransient<ShopViewModel>();
        builder.Services.AddTransient<ShopPage>();
        builder.Services.AddTransient<CartViewModel>();
        builder.Services.AddTransient<CartPage>();

        builder.Services.AddTransient<PromotionsViewModel>();
        builder.Services.AddTransient<PromotionsPage>();
        builder.Services.AddTransient<ServiceInfoViewModel>();
        builder.Services.AddTransient<ServiceInfoPage>();

        builder.Services.AddTransient<MyRequestsViewModel>();
        builder.Services.AddTransient<MyRequestsPage>();
        builder.Services.AddTransient<PriceListViewModel>();
        builder.Services.AddTransient<PriceListPage>();

        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<ContactUsViewModel>();
        builder.Services.AddTransient<ContactUsPage>();

//#if DEBUG
//        builder.Logging.AddDebug();
//#endif

        return builder.Build();
    }
}
