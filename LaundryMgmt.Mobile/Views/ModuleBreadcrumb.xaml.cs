namespace LaundryMgmt.Mobile.Views;

/// <summary>
/// Small "you are here" strip shown at the top of each management page
/// (Orders/Customers/Garments/Services/Pricing) so you can jump straight to
/// another module without hunting for the platform-specific tab bar.
/// </summary>
public partial class ModuleBreadcrumb : ContentView
{
    public static readonly BindableProperty CurrentModuleProperty = BindableProperty.Create(
        nameof(CurrentModule), typeof(string), typeof(ModuleBreadcrumb), string.Empty,
        propertyChanged: (bindable, _, newValue) => ((ModuleBreadcrumb)bindable).ApplyActiveState((string)newValue));

    public string CurrentModule
    {
        get => (string)GetValue(CurrentModuleProperty);
        set => SetValue(CurrentModuleProperty, value);
    }

    public ModuleBreadcrumb()
    {
        InitializeComponent();

        // Customers/Garments/Services are staff-only (see AppShell.BuildTabsForRole and
        // the matching API-side [Authorize(Roles=...)] restrictions) — a customer login
        // would just hit a 403 tapping them, so hide them for that role. Reads the same
        // Preferences key AuthService.Role does; no DI available for a plain XAML-declared
        // ContentView, so this mirrors that lookup directly rather than injecting the service.
        var role = Preferences.Default.Get<string?>("auth_role", null);
        var isCustomer = role == "Customer";
        CustomersChip.IsVisible = !isCustomer;
        GarmentsChip.IsVisible = !isCustomer;
        ServicesChip.IsVisible = !isCustomer;
        UsersChip.IsVisible = role == "Admin";
    }

    private void ApplyActiveState(string currentModule)
    {
        SetChipState(OrdersChip, OrdersLabel, currentModule == "orders");
        SetChipState(CustomersChip, CustomersLabel, currentModule == "customers");
        SetChipState(GarmentsChip, GarmentsLabel, currentModule == "garments");
        SetChipState(ServicesChip, ServicesLabel, currentModule == "services");
        SetChipState(PricingChip, PricingLabel, currentModule == "pricing");
        SetChipState(UsersChip, UsersLabel, currentModule == "users");
    }

    private static void SetChipState(Border chip, Label label, bool isActive)
    {
        chip.BackgroundColor = isActive
            ? (Color)Application.Current!.Resources["Primary"]
            : (Color)Application.Current!.Resources["PrimaryLight"];
        label.TextColor = isActive
            ? (Color)Application.Current!.Resources["White"]
            : (Color)Application.Current!.Resources["Primary"];
        label.FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None;
    }

    private async void OnOrdersTapped(object? sender, EventArgs e) => await NavigateAsync("orders");
    private async void OnCustomersTapped(object? sender, EventArgs e) => await NavigateAsync("customers");
    private async void OnGarmentsTapped(object? sender, EventArgs e) => await NavigateAsync("garments");
    private async void OnServicesTapped(object? sender, EventArgs e) => await NavigateAsync("services");
    private async void OnPricingTapped(object? sender, EventArgs e) => await NavigateAsync("pricing");
    private async void OnUsersTapped(object? sender, EventArgs e) => await NavigateAsync("users");

    private static async Task NavigateAsync(string route)
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync($"//{route}");
    }
}
