namespace LaundryMgmt.Mobile.Views;

/// <summary>Horizontally scrollable top nav for the Customer role's 9 destinations —
/// replaces Shell's bottom TabBar for these pages (see Shell.SetTabBarIsVisible calls
/// in each customer page's code-behind), which on Android caps visible tabs at ~5 and
/// buries the rest under a "More" overflow menu. A full-width scrollable strip avoids
/// that cap entirely and reads better for an 8-destination e-commerce-style nav.</summary>
public partial class CustomerNavBar : ContentView
{
    public static readonly BindableProperty CurrentPageProperty = BindableProperty.Create(
        nameof(CurrentPage), typeof(string), typeof(CustomerNavBar), string.Empty,
        propertyChanged: (bindable, _, newValue) => ((CustomerNavBar)bindable).ApplyActiveState((string)newValue));

    public string CurrentPage
    {
        get => (string)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public CustomerNavBar() => InitializeComponent();

    private void ApplyActiveState(string currentPage)
    {
        SetChipState(ShopChip, ShopLabel, currentPage == "shop");
        SetChipState(ServiceInfoChip, ServiceInfoLabel, currentPage == "serviceinfo");
        SetChipState(PromotionsChip, PromotionsLabel, currentPage == "promotions");
        SetChipState(MyRequestsChip, MyRequestsLabel, currentPage == "myrequests");
        SetChipState(OrdersChip, OrdersLabel, currentPage == "orders");
        SetChipState(PriceListChip, PriceListLabel, currentPage == "pricelist");
        SetChipState(MembershipChip, MembershipLabel, currentPage == "membership");
        SetChipState(SettingsChip, SettingsLabel, currentPage == "settings");
        SetChipState(ContactUsChip, ContactUsLabel, currentPage == "contactus");
    }

    private static void SetChipState(Border chip, Label label, bool isActive)
    {
        chip.BackgroundColor = isActive
            ? (Color)Application.Current!.Resources["Primary"]
            : (Color)Application.Current!.Resources["PrimaryLight"];
        label.TextColor = isActive
            ? (Color)Application.Current!.Resources["White"]
            : (Color)Application.Current!.Resources["Gray600"];
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

    private static async Task NavigateAsync(string route)
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync($"//{route}");
    }
}
