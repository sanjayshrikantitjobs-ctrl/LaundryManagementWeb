using LaundryMgmt.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LaundryMgmt.Mobile.Views;

public partial class RegisterPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RegisterViewModel _viewModel;

    public RegisterPage(IServiceProvider serviceProvider, RegisterViewModel viewModel)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.Registered += OnRegistered;
    }

    private void OnRegistered(string phoneNumber)
    {
        var verifyPage = _serviceProvider.GetRequiredService<VerifyOtpPage>();
        verifyPage.SetPhoneNumber(phoneNumber);
        SwapWindowPage(verifyPage);
    }

    private void OnSignInTapped(object? sender, EventArgs e) =>
        SwapWindowPage(_serviceProvider.GetRequiredService<LoginPage>());

    private static void SwapWindowPage(Page page)
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null) window.Page = page;
    }
}
