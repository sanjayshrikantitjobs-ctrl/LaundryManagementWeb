using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LaundryMgmt.Mobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthService _authService;
    private readonly LoginViewModel _viewModel;

    public LoginPage(IServiceProvider serviceProvider, AuthService authService, LoginViewModel viewModel)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _authService = authService;
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.LoggedIn += GoToAppShell;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // If a session is already saved (app was closed and reopened), skip straight
        // to the Shell instead of making the user log in again.
        if (await _authService.TryRestoreSessionAsync())
            GoToAppShell();
    }

    private void GoToAppShell()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
            window.Page = _serviceProvider.GetRequiredService<AppShell>();
    }

    private void OnCreateAccountTapped(object? sender, EventArgs e)
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
            window.Page = _serviceProvider.GetRequiredService<RegisterPage>();
    }
}
