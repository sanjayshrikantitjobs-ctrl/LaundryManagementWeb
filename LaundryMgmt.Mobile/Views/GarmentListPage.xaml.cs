using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

/// <summary>The garment catalogue for whichever service the customer just tapped on
/// ShopPage — split out from ShopPage itself (which now only handles the
/// category/service pickers) so the two levels of the browse flow each get a full
/// screen instead of being crammed into one scroll. Shares ShopViewModel (a singleton)
/// with ShopPage — no separate fetch/state here.</summary>
public partial class GarmentListPage : ContentPage
{
    private readonly ShopViewModel _viewModel;

    public GarmentListPage(ShopViewModel viewModel)
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
