using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

public partial class ShopPage : ContentPage
{
    private readonly ShopViewModel _viewModel;
    private IDispatcherTimer? _heroTimer;

    public ShopPage(ShopViewModel viewModel)
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

        StartHeroAutoSlide();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Stop the timer while the page isn't visible — otherwise it keeps firing and
        // touching HeroCarousel from other tabs/pages, which leaks a timer per visit.
        StopHeroAutoSlide();
    }

    private void StartHeroAutoSlide()
    {
        if (_heroTimer is not null || _viewModel.HeroSlides.Count <= 1) return;

        _heroTimer = Dispatcher.CreateTimer();
        _heroTimer.Interval = TimeSpan.FromSeconds(7);
        _heroTimer.Tick += (_, _) =>
        {
            var count = _viewModel.HeroSlides.Count;
            HeroCarousel.Position = (HeroCarousel.Position + 1) % count;
        };
        _heroTimer.Start();
    }

    private void StopHeroAutoSlide()
    {
        _heroTimer?.Stop();
        _heroTimer = null;
    }
}
