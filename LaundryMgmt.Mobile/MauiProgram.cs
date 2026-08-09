using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels;
using LaundryMgmt.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace LaundryMgmt.Mobile;

public static class MauiProgram
{
    // Talks to the HTTPS endpoint directly (not the HTTP one) so the API's
    // UseHttpsRedirection() middleware never gets a chance to 307-redirect the
    // request — HttpClient follows redirects and, like a browser, drops the
    // Authorization header on a cross-origin redirect, which silently turns every
    // authenticated call into an anonymous one (see client-web/proxy.conf.json for
    // the same issue on the Angular side).
    public static string ApiBaseUrl =>
#if ANDROID
        "https://10.0.2.2:5101";
#else
        "https://localhost:5101";
#endif

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
        builder.Services.AddTransient<DeliveryConfirmationViewModel>();
        builder.Services.AddTransient<DeliveryConfirmationPage>();

        builder.Services.AddTransient<OrdersViewModel>();
        builder.Services.AddTransient<OrdersPage>();
        builder.Services.AddTransient<OrderFormViewModel>();
        builder.Services.AddTransient<OrderFormPage>();

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

        builder.Services.AddTransient<PricingViewModel>();
        builder.Services.AddTransient<PricingPage>();

//#if DEBUG
//        builder.Logging.AddDebug();
//#endif

        return builder.Build();
    }
}
