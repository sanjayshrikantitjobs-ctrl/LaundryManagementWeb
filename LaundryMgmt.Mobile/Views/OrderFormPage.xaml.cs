using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

public partial class OrderFormPage : ContentPage
{
    private readonly OrderFormViewModel _viewModel;

    public OrderFormPage(OrderFormViewModel viewModel)
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
            // Belt-and-braces: InitializeAsync already catches its own errors, but an
            // unhandled exception reaching this async void method would crash the app.
            await DisplayAlert("Something went wrong", ex.Message, "OK");
        }
    }
}
