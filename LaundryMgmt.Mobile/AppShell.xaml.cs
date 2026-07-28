using LaundryMgmt.Mobile.Views;

namespace LaundryMgmt.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(DeliveryConfirmationPage), typeof(DeliveryConfirmationPage));
    }
}
