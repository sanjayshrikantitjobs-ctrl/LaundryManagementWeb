using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

public partial class CustomersPage : ContentPage
{
    private readonly CustomersViewModel _viewModel;

    public CustomersPage(CustomersViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
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
