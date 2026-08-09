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

        Routing.RegisterRoute(nameof(DeliveryConfirmationPage), typeof(DeliveryConfirmationPage));
        Routing.RegisterRoute(nameof(OrderFormPage), typeof(OrderFormPage));
        Routing.RegisterRoute(nameof(CustomerFormPage), typeof(CustomerFormPage));
        Routing.RegisterRoute(nameof(GarmentFormPage), typeof(GarmentFormPage));
        Routing.RegisterRoute(nameof(ServiceFormPage), typeof(ServiceFormPage));

        BuildTabsForRole(authService.Role);
    }

    private void BuildTabsForRole(string? role)
    {
        var tabBar = new TabBar();

        var isManagement = role is "Admin" or "StoreManager" or "Staff";
        var isDeliveryBoy = role == "DeliveryBoy";
        var isCustomer = role == "Customer";

        if (isManagement || isCustomer)
            tabBar.Items.Add(CreateTab(isCustomer ? "My Orders" : "Orders", "orders", typeof(OrdersPage)));

        if (isManagement)
        {
            tabBar.Items.Add(CreateTab("Customers", "customers", typeof(CustomersPage)));
            tabBar.Items.Add(CreateTab("Garments", "garments", typeof(GarmentsPage)));
            tabBar.Items.Add(CreateTab("Services", "services", typeof(ServicesPage)));
        }

        if (isManagement || isCustomer)
            tabBar.Items.Add(CreateTab("Pricing", "pricing", typeof(PricingPage)));

        if (isManagement || isDeliveryBoy)
            tabBar.Items.Add(CreateTab("My Queue", "queue", typeof(OrderQueuePage)));

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
