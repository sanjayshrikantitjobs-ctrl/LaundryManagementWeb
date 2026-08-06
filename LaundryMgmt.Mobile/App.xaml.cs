using LaundryMgmt.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LaundryMgmt.Mobile;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
        new(_serviceProvider.GetRequiredService<LoginPage>());
}
