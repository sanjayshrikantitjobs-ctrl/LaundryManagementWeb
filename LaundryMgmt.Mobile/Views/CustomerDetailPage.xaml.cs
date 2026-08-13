using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

[QueryProperty(nameof(CustomerId), "customerId")]
public partial class CustomerDetailPage : ContentPage
{
    private readonly CustomerDetailViewModel _viewModel;

    public string CustomerId
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
                _viewModel.CustomerId = guid;
        }
    }

    public CustomerDetailPage(CustomerDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
