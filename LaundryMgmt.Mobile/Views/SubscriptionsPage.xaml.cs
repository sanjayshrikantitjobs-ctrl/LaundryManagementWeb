using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

public partial class SubscriptionsPage : ContentPage
{
    private readonly SubscriptionsViewModel _viewModel;

    public SubscriptionsPage(SubscriptionsViewModel viewModel, AuthService authService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Shared between management ("Subscriptions") and Customer ("Membership") — same
        // dual-chrome pattern as OrdersPage: each role gets its own hamburger-driven
        // drawer overlay.
        if (authService.Role == "Customer")
            CustomerDrawerOverlay.IsVisible = true;
        else
            AdminDrawerOverlay.IsVisible = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Something went wrong", ex.Message, "OK");
        }
    }
}
