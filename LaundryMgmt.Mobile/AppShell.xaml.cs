using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LaundryMgmt.Mobile;

public partial class AppShell : Shell
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthService _authService;

    public AppShell(IServiceProvider serviceProvider, AuthService authService)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _authService = authService;

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

        GreetingLabel.Text = BuildGreeting(authService.FullName);
        BuildTabsForRole(authService.Role);
    }

    private static string BuildGreeting(string? fullName)
    {
        var timeOfDay = DateTime.Now.Hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            _ => "Good evening"
        };

        return string.IsNullOrWhiteSpace(fullName) ? timeOfDay : $"{timeOfDay}, {fullName}";
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
