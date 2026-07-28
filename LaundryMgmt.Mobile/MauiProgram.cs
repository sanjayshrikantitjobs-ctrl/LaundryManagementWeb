using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels;
using LaundryMgmt.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace LaundryMgmt.Mobile;

public static class MauiProgram
{
    // Point this at the deployed API. For local Android emulator debugging,
    // use http://10.0.2.2:5100 instead of localhost.
    public const string ApiBaseUrl = "https://api.laundrymgmt.example.com";

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
        builder.Services.AddSingleton<DeliveryOrderHubService>();

        builder.Services.AddTransient<OrderQueueViewModel>();
        builder.Services.AddTransient<OrderQueuePage>();
        builder.Services.AddTransient<DeliveryConfirmationViewModel>();
        builder.Services.AddTransient<DeliveryConfirmationPage>();

//#if DEBUG
//        builder.Logging.AddDebug();
//#endif

        return builder.Build();
    }
}
