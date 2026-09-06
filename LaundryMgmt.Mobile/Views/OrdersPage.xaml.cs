using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

public partial class OrdersPage : ContentPage
{
    private readonly OrdersViewModel _viewModel;

    public OrdersPage(OrdersViewModel viewModel, AuthService authService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // OrdersPage is shared between Customer ("My Orders") and management roles
        // ("Orders") — both lose the bottom TabBar (avoids Android's "More" overflow
        // once a role has more than ~5 tabs) in favor of the hamburger-driven drawer;
        // which drawer overlay is shown depends on which chrome the role uses.
        if (authService.Role == "Customer")
            CustomerDrawerOverlay.IsVisible = true;
        else
            AdminDrawerOverlay.IsVisible = true;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RefreshCommand.Execute(null);
        _viewModel.RefreshCountsCommand.Execute(null);
    }

    /// <summary>Purely decorative press-feedback for a status filter chip — a quick
    /// scale bounce alongside the TapGestureRecognizer's Command (which still drives
    /// SetTabCommand/ActiveTab exactly as before). Doesn't touch any binding or command.</summary>
    private static async void OnStatusChipTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not VisualElement chip) return;
        await chip.ScaleTo(0.95, 60, Easing.CubicOut);
        await chip.ScaleTo(1.0, 90, Easing.CubicOut);
    }
}
