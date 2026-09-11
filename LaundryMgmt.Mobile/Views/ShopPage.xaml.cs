using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

public partial class ShopPage : ContentPage
{
    // Matches the bundled hero banners' shared aspect ratio (all normalized to this in
    // Resources/Images) — keep in sync if a banner asset is ever replaced.
    private const double HeroAspectRatio = 2070.0 / 760.0;

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
        _heroTimer.Interval = TimeSpan.FromSeconds(5);
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

    /// <summary>Keeps the carousel box's own aspect ratio equal to the banners' ratio on
    /// every arrange pass (rotation, window resize, first layout) — a fixed HeightRequest
    /// can't do this since the box's width varies by device, which is what produced the
    /// letterbox bars regardless of how well the image assets matched each other.</summary>
    private void OnHeroCarouselSizeChanged(object? sender, EventArgs e)
    {
        if (HeroCarousel.Width <= 0) return;

        var targetHeight = HeroCarousel.Width / HeroAspectRatio;
        if (Math.Abs(HeroCarousel.HeightRequest - targetHeight) > 0.5)
            HeroCarousel.HeightRequest = targetHeight;
    }
}
