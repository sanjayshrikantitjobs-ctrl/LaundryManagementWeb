using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

public partial class ServiceInfoPage : ContentPage
{
    public ServiceInfoPage(ServiceInfoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
