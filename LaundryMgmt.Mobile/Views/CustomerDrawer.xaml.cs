using LaundryMgmt.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LaundryMgmt.Mobile.Views;

/// <summary>Left slide-out navigation drawer for the Customer role — the same 9
/// destinations the old horizontal chip-strip nav used to expose, now behind
/// a hamburger button in the global title bar (see AppShell.OnMenuTapped). Each
/// customer page hosts one instance of this view as a full-bleed overlay; open/close
/// state is shared via <see cref="CustomerDrawerService"/> so the single hamburger
/// button can drive whichever page is currently on screen.</summary>
public partial class CustomerDrawer : ContentView
{
    public static readonly BindableProperty CurrentPageProperty = BindableProperty.Create(
        nameof(CurrentPage), typeof(string), typeof(CustomerDrawer), string.Empty,
        propertyChanged: (bindable, _, newValue) => ((CustomerDrawer)bindable).ApplyActiveState((string)newValue));

    public string CurrentPage
    {
        get => (string)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    private readonly CustomerDrawerService? _drawerService;
    private readonly AuthService? _authService;
    private bool _isAnimating;

    public CustomerDrawer()
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
        SetItemState(ShopItem, ShopLabel, currentPage == "shop");
        SetItemState(ServiceInfoItem, ServiceInfoLabel, currentPage == "serviceinfo");
        SetItemState(PromotionsItem, PromotionsLabel, currentPage == "promotions");
        SetItemState(MyRequestsItem, MyRequestsLabel, currentPage == "myrequests");
        SetItemState(OrdersItem, OrdersLabel, currentPage == "orders");
        SetItemState(PriceListItem, PriceListLabel, currentPage == "pricelist");
        SetItemState(MembershipItem, MembershipLabel, currentPage == "membership");
        SetItemState(SettingsItem, SettingsLabel, currentPage == "settings");
        SetItemState(ContactUsItem, ContactUsLabel, currentPage == "contactus");
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

    private async void OnShopTapped(object? sender, EventArgs e) => await NavigateAsync("ShopPage");
    private async void OnServiceInfoTapped(object? sender, EventArgs e) => await NavigateAsync("ServiceInfoPage");
    private async void OnPromotionsTapped(object? sender, EventArgs e) => await NavigateAsync("PromotionsPage");
    private async void OnMyRequestsTapped(object? sender, EventArgs e) => await NavigateAsync("MyRequestsPage");
    private async void OnOrdersTapped(object? sender, EventArgs e) => await NavigateAsync("orders");
    private async void OnPriceListTapped(object? sender, EventArgs e) => await NavigateAsync("PriceListPage");
    private async void OnMembershipTapped(object? sender, EventArgs e) => await NavigateAsync("SubscriptionsPage");
    private async void OnSettingsTapped(object? sender, EventArgs e) => await NavigateAsync("SettingsPage");
    private async void OnContactUsTapped(object? sender, EventArgs e) => await NavigateAsync("ContactUsPage");

    private async Task NavigateAsync(string route)
    {
        _drawerService?.Close();
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync($"//{route}");
    }
}
