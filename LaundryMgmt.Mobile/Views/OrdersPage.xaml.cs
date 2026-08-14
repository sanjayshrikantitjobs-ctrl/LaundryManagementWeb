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
        // ("Orders") — both lose the bottom TabBar in favor of a top nav strip (avoids
        // Android's "More" overflow once a role has more than ~5 tabs); only the top
        // strip's content differs (scrollable CustomerNavBar vs. ModuleBreadcrumb).
        if (authService.Role == "Customer")
        {
            ManagementNavBar.IsVisible = false;
            CustomerNav.IsVisible = true;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RefreshCommand.Execute(null);
    }
}
