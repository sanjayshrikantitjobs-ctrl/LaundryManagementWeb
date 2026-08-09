using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

public partial class PricingPage : ContentPage
{
    private readonly PricingViewModel _viewModel;

    public PricingPage(PricingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
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
