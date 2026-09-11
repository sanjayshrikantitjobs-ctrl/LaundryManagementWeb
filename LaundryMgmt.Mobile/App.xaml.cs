using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;
using Plugin.FirebasePushNotifications;

namespace LaundryMgmt.Mobile;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider, IFirebasePushNotification pushNotification)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;

        // Subscribed here (App's constructor runs before any Page/Shell), not on a
        // page — the plugin queues a tap that arrives before a handler is attached
        // (e.g. the app was closed/cold-started by tapping the notification) and
        // replays it the moment this subscription attaches, so wiring it this early
        // is what makes the cold-start case work at all.
        pushNotification.NotificationOpened += OnPushNotificationOpened;
    }

    private async void OnPushNotificationOpened(object? sender, FirebasePushNotificationResponseEventArgs e)
    {
        if (!e.Data.TryGetValue("type", out var rawType) || rawType?.ToString() is not { } typeText) return;
        if (!Enum.TryParse<NotificationType>(typeText, out var type)) return;

        e.Data.TryGetValue("entityId", out var rawEntityId);
        var route = NotificationRouting.BuildRoute(type, rawEntityId?.ToString());
        if (route is null) return;

        // Shell might not be up yet on a genuine cold start; a couple of short
        // retries covers that window without risking an infinite wait.
        for (var attempt = 0; attempt < 10 && Shell.Current is null; attempt++)
            await Task.Delay(300);

        await SafeNavigation.GoToAsync(route);
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
        new(_serviceProvider.GetRequiredService<LoginPage>());
}
