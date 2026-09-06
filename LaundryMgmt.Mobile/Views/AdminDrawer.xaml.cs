using LaundryMgmt.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LaundryMgmt.Mobile.Views;

/// <summary>Left slide-out navigation drawer for management roles (Admin,
/// StoreManager, Staff, DepartmentHead) — replaces the old horizontal
/// ModuleBreadcrumb chip-strip with the same hamburger-driven drawer pattern the
/// Customer role already uses. Each management page hosts one instance of this view
/// as a full-bleed overlay; open/close state is shared via <see cref="CustomerDrawerService"/>
/// (the same singleton the Customer drawer uses — only one drawer is ever mounted and
/// visible per page, gated by role, so sharing the toggle state is safe) so the single
/// hamburger button in the global title bar can drive whichever page is on screen.</summary>
public partial class AdminDrawer : ContentView
{
    public static readonly BindableProperty CurrentPageProperty = BindableProperty.Create(
        nameof(CurrentPage), typeof(string), typeof(AdminDrawer), string.Empty,
        propertyChanged: (bindable, _, newValue) => ((AdminDrawer)bindable).ApplyActiveState((string)newValue));

    public string CurrentPage
    {
        get => (string)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    private readonly CustomerDrawerService? _drawerService;
    private readonly AuthService? _authService;
    private bool _isAnimating;

    public AdminDrawer()
    {
        InitializeComponent();

        var services = IPlatformApplication.Current?.Services;
        _drawerService = services?.GetService<CustomerDrawerService>();
        _authService = services?.GetService<AuthService>();

        Panel.TranslationX = -300;

        if (_drawerService is not null)
            _drawerService.PropertyChanged += (_, _) => ApplyOpenState(_drawerService.IsOpen);

        if (!string.IsNullOrWhiteSpace(_authService?.FullName))
            UserNameLabel.Text = $"Hi, {_authService.FullName}";

        // Users management is Admin-only (see AppShell.BuildTabsForRole and the
        // matching API-side [Authorize(Roles="Admin")] restriction) — StoreManager,
        // Staff and DepartmentHead would just hit a 403 tapping it.
        UsersItem.IsVisible = _authService?.Role == "Admin";

        ApplyActiveState(CurrentPage);
    }

    private async void ApplyOpenState(bool isOpen)
    {
        if (_isAnimating) return;
        _isAnimating = true;

        try
        {
            if (isOpen)
            {
                Backdrop.InputTransparent = false;
                await Task.WhenAll(
                    Backdrop.FadeTo(0.4, 200),
                    Panel.TranslateTo(0, 0, 250, Easing.CubicOut));
            }
            else
            {
                await Task.WhenAll(
                    Backdrop.FadeTo(0, 180),
                    Panel.TranslateTo(-300, 0, 220, Easing.CubicIn));
                Backdrop.InputTransparent = true;
            }
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private void OnBackdropTapped(object? sender, EventArgs e) => _drawerService?.Close();

    private void ApplyActiveState(string currentPage)
    {
        SetItemState(OrdersItem, OrdersLabel, currentPage == "orders");
        SetItemState(CustomersItem, CustomersLabel, currentPage == "customers");
        SetItemState(GarmentsItem, GarmentsLabel, currentPage == "garments");
        SetItemState(ServicesItem, ServicesLabel, currentPage == "services");
        SetItemState(PricingItem, PricingLabel, currentPage == "pricing");
        SetItemState(SubscriptionsItem, SubscriptionsLabel, currentPage == "subscriptions");
        SetItemState(UsersItem, UsersLabel, currentPage == "users");
    }

    private static void SetItemState(Border item, Label label, bool isActive)
    {
        item.BackgroundColor = isActive
            ? (Color)Application.Current!.Resources["PrimaryLight"]
            : Colors.Transparent;
        label.TextColor = isActive
            ? (Color)Application.Current!.Resources["Primary"]
            : (Color)Application.Current!.Resources["Gray900"];
        label.FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None;
    }

    private async void OnOrdersTapped(object? sender, EventArgs e) => await NavigateAsync("orders");
    private async void OnCustomersTapped(object? sender, EventArgs e) => await NavigateAsync("customers");
    private async void OnGarmentsTapped(object? sender, EventArgs e) => await NavigateAsync("garments");
    private async void OnServicesTapped(object? sender, EventArgs e) => await NavigateAsync("services");
    private async void OnPricingTapped(object? sender, EventArgs e) => await NavigateAsync("pricing");
    private async void OnSubscriptionsTapped(object? sender, EventArgs e) => await NavigateAsync("subscriptions");
    private async void OnUsersTapped(object? sender, EventArgs e) => await NavigateAsync("users");

    private async Task NavigateAsync(string route)
    {
        _drawerService?.Close();
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync($"//{route}");
    }
}
