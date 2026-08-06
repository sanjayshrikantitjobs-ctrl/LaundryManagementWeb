using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

public partial class CustomerFormPage : ContentPage
{
    public CustomerFormPage(CustomerFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
