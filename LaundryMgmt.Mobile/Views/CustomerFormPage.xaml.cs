using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

[QueryProperty(nameof(CustomerId), "customerId")]
public partial class CustomerFormPage : ContentPage
{
    private readonly CustomerFormViewModel _viewModel;

    public string CustomerId
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
                _viewModel.CustomerId = guid;
        }
    }

    public CustomerFormPage(CustomerFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
