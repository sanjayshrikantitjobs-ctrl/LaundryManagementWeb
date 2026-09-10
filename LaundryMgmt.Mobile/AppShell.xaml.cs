using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels;
using LaundryMgmt.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LaundryMgmt.Mobile;

public partial class AppShell : Shell
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthService _authService;
    private readonly ApiClient _apiClient;
    private readonly CustomerDrawerService _drawerService;
    private IDispatcherTimer? _unreadPollTimer;

    public AppShell(IServiceProvider serviceProvider, AuthService authService, ApiClient apiClient, CustomerDrawerService drawerService)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _authService = authService;
        _apiClient = apiClient;
        _drawerService = drawerService;

        // Hamburger + drawer nav for Customer and every management role (Admin,
        // StoreManager, Staff, DepartmentHead) — each hosts its own drawer overlay
        // (CustomerDrawer or AdminDrawer) driven by the same CustomerDrawerService.
        // PickupAgent/DeliveryAgent keep their single-tab queue view with no drawer.
        MenuButton.IsVisible = authService.Role is "Customer" or "Admin" or "StoreManager" or "Staff" or "DepartmentHead";

        Routing.RegisterRoute(nameof(OrderFormPage), typeof(OrderFormPage));
        Routing.RegisterRoute(nameof(OrderDetailPage), typeof(OrderDetailPage));
        Routing.RegisterRoute(nameof(CustomerFormPage), typeof(CustomerFormPage));
        Routing.RegisterRoute(nameof(CustomerDetailPage), typeof(CustomerDetailPage));
        Routing.RegisterRoute(nameof(GarmentDetailPage), typeof(GarmentDetailPage));
        Routing.RegisterRoute(nameof(UserDetailPage), typeof(UserDetailPage));
        Routing.RegisterRoute(nameof(ServiceDetailPage), typeof(ServiceDetailPage));
        Routing.RegisterRoute(nameof(GarmentFormPage), typeof(GarmentFormPage));
        Routing.RegisterRoute(nameof(ServiceFormPage), typeof(ServiceFormPage));
        Routing.RegisterRoute(nameof(UserFormPage), typeof(UserFormPage));
        Routing.RegisterRoute(nameof(CartPage), typeof(CartPage));
        Routing.RegisterRoute(nameof(SubscriptionPlanFormPage), typeof(SubscriptionPlanFormPage));
        Routing.RegisterRoute(nameof(NotificationsPage), typeof(NotificationsPage));
        Routing.RegisterRoute(nameof(GarmentListPage), typeof(GarmentListPage));

        GreetingLabel.Text = BuildGreeting(authService.FullName);
        BuildTabsForRole(authService.Role);

        Loaded += (_, _) => StartUnreadPolling();

        // Every GoToAsync in the app — row taps, drawer items, form saves that
        // navigate away, etc. — flows through Shell's own navigation pipeline, so
        // hooking these two events here is enough to show a "something is happening"
        // indicator for literally every navigation, without touching any of the
        // ~25 ViewModels that call GoToAsync themselves.
        Navigating += OnShellNavigating;
        Navigated += OnShellNavigated;
    }

    private uint _loadingBarToken;
    private DateTime _loadingBarShownAtUtc;

    private async void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        var token = ++_loadingBarToken;
        LoadingBar.CancelAnimations();
        LoadingBar.ScaleX = 0;
        LoadingBar.IsVisible = true;
        _loadingBarShownAtUtc = DateTime.UtcNow;
        // Eases up to ~75% and holds — never claims to be "done" before the
        // destination page has actually finished loading its own data (Navigated,
        // below, still owns the trip to 100%).
        await LoadingBar.ScaleXTo(0.75, 550, Easing.CubicOut);
        _ = token; // CancelAnimations already stops this if a newer navigation started
    }

    private async void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        var token = ++_loadingBarToken;

        // A navigation resolved entirely from already-cached data (e.g. reopening a
        // page whose ViewModel already loaded once) can finish in well under 100ms —
        // too fast for the bar to register as visible feedback at all. Hold it on
        // screen for a short minimum so it's still noticeable without adding a
        // perceptible extra wait on top of the actual navigation.
        var elapsed = DateTime.UtcNow - _loadingBarShownAtUtc;
        var minVisible = TimeSpan.FromMilliseconds(150);
        if (elapsed < minVisible)
            await Task.Delay(minVisible - elapsed);
        if (token != _loadingBarToken) return; // a newer navigation started meanwhile

        LoadingBar.CancelAnimations();
        await LoadingBar.ScaleXTo(1.0, 150, Easing.CubicOut);
        await Task.Delay(120);
        if (token != _loadingBarToken) return; // a newer navigation started meanwhile
        LoadingBar.IsVisible = false;
        LoadingBar.ScaleX = 0;
    }

    // Mirrors client-web's notification-bell polling (25s interval, see
    // notification-bell.component.ts) so the badge count in the title bar stays fresh
    // without the user needing to open the notifications page.
    private void StartUnreadPolling()
    {
        _unreadPollTimer = Dispatcher.CreateTimer();
        _unreadPollTimer.Interval = TimeSpan.FromSeconds(25);
        _unreadPollTimer.Tick += async (_, _) => await RefreshUnreadCountAsync();
        _unreadPollTimer.Start();
        _ = RefreshUnreadCountAsync();
    }

    private async Task RefreshUnreadCountAsync()
    {
        try
        {
            var count = _authService.Role is null ? 0 : await _apiClient.GetUnreadNotificationCountAsync();
            UnreadBadgeLabel.Text = count > 9 ? "9+" : count.ToString();
            UnreadBadge.IsVisible = count > 0;
        }
        catch
        {
            // Best-effort — a failed poll just leaves the badge as it was.
        }
    }

    private async void OnNotificationsTapped(object? sender, EventArgs e) =>
        await GoToAsync(nameof(NotificationsPage));

    private void OnMenuTapped(object? sender, EventArgs e) => _drawerService.Toggle();

    private static async void OnCallUsTapped(object? sender, EventArgs e)
    {
        try
        {
            await Launcher.Default.OpenAsync(new Uri($"tel:{ContactUsViewModel.SupportPhoneNumber}"));
        }
        catch
        {
            // Best-effort — no dialer available on this device/emulator.
        }
    }

    // Keeps the title bar from being crowded out by a long name (e.g. "Good evening,
    // System Administrator") — just the first name, and only its first 8 characters
    // if even that alone is long.
    private static string BuildGreeting(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "Hello";

        var firstName = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? fullName;
        if (firstName.Length > 8) firstName = firstName[..8];

        return $"Hello, {firstName}";
    }

    private void BuildTabsForRole(string? role)
    {
        var tabBar = new TabBar();

        // DepartmentHead gets the same tab set as Admin/StoreManager/Staff — view
        // access to Orders/Customers/Garments/Services/Pricing, same as the web app's
        // nav. Write actions (Add/Edit/Delete, price changes) are gated per-page by
        // CanEditMasterData/CanEditPrice, not by hiding the tab — matches
        // AppRoles.OperationalViewRoles on the API (Management + DepartmentHead can
        // all read this data; only AppRoles.ManagementRoles, which excludes
        // DepartmentHead, can write it).
        var isManagement = role is "Admin" or "StoreManager" or "Staff" or "DepartmentHead";
        var isPickupAgent = role == "PickupAgent";
        var isDeliveryAgent = role == "DeliveryAgent";
        var isCustomer = role == "Customer";

        if (isCustomer)
        {
            tabBar.Items.Add(CreateTab("Home", "ShopPage", typeof(ShopPage)));
            tabBar.Items.Add(CreateTab("Services Info", "ServiceInfoPage", typeof(ServiceInfoPage)));
            tabBar.Items.Add(CreateTab("Promotions", "PromotionsPage", typeof(PromotionsPage)));
            tabBar.Items.Add(CreateTab("My Requests", "MyRequestsPage", typeof(MyRequestsPage)));
        }

        if (isManagement || isCustomer)
            tabBar.Items.Add(CreateTab(isCustomer ? "My Orders" : "Orders", "orders", typeof(OrdersPage)));

        if (isManagement)
        {
            tabBar.Items.Add(CreateTab("Customers", "customers", typeof(CustomersPage)));
            tabBar.Items.Add(CreateTab("Garments", "garments", typeof(GarmentsPage)));
            tabBar.Items.Add(CreateTab("Services", "services", typeof(ServicesPage)));
            tabBar.Items.Add(CreateTab("Pricing", "pricing", typeof(PricingMatrixPage)));
            tabBar.Items.Add(CreateTab("Subscriptions", "subscriptions", typeof(SubscriptionsPage)));
        }

        if (isCustomer)
        {
            tabBar.Items.Add(CreateTab("Price List", "PriceListPage", typeof(PriceListPage)));
            tabBar.Items.Add(CreateTab("Membership", "SubscriptionsPage", typeof(SubscriptionsPage)));
            tabBar.Items.Add(CreateTab("Settings", "SettingsPage", typeof(SettingsPage)));
            tabBar.Items.Add(CreateTab("Contact Us", "ContactUsPage", typeof(ContactUsPage)));
        }

        // PickupDeliveryController.GetMine is Authorize(Roles = "PickupAgent,DeliveryAgent")
        // only — management roles never had assignments to see here (the original
        // "isManagement || isDeliveryBoy" condition on this tab was harmless only
        // because OrderQueueViewModel used to be a stub; now that it calls the real
        // endpoint, showing this tab to management would just produce a 403).
        if (isPickupAgent || isDeliveryAgent)
            tabBar.Items.Add(CreateTab(isPickupAgent ? "My Pickup Queue" : "My Delivery Queue", "queue", typeof(OrderQueuePage)));

        if (role == "Admin")
            tabBar.Items.Add(CreateTab("Users", "users", typeof(UsersPage)));

        Items.Add(tabBar);
    }

    private ShellContent CreateTab(string title, string route, Type pageType) => new()
    {
        Title = title,
        Route = route,
        ContentTemplate = new DataTemplate(() => _serviceProvider.GetRequiredService(pageType))
    };

    private void OnLogoutClicked(object? sender, EventArgs e)
    {
        _authService.Logout();
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
            window.Page = _serviceProvider.GetRequiredService<LoginPage>();
    }
}
