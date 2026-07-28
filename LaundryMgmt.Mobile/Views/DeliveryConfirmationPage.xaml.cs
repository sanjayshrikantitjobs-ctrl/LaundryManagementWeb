using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

[QueryProperty(nameof(OrderId), "orderId")]
public partial class DeliveryConfirmationPage : ContentPage
{
    private readonly DeliveryConfirmationViewModel _viewModel;

    public string OrderId
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
                _viewModel.OrderId = guid;
        }
    }

    public DeliveryConfirmationPage(DeliveryConfirmationViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
