using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

public partial class ServiceFormPage : ContentPage
{
    public ServiceFormPage(ServiceFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
