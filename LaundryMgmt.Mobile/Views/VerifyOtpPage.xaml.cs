using LaundryMgmt.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LaundryMgmt.Mobile.Views;

public partial class VerifyOtpPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly VerifyOtpViewModel _viewModel;

    public VerifyOtpPage(IServiceProvider serviceProvider, VerifyOtpViewModel viewModel)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.Verified += OnVerified;
    }

    public void SetPhoneNumber(string phoneNumber) => _viewModel.PhoneNumber = phoneNumber;

    private void OnVerified()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
            window.Page = _serviceProvider.GetRequiredService<AppShell>();
    }
}
