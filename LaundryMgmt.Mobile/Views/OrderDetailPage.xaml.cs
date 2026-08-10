using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

[QueryProperty(nameof(OrderId), "orderId")]
[QueryProperty(nameof(PickupId), "pickupId")]
[QueryProperty(nameof(DeliveryId), "deliveryId")]
public partial class OrderDetailPage : ContentPage
{
    private readonly OrderDetailViewModel _viewModel;

    public string OrderId
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
                _ = _viewModel.LoadAsync(guid);
        }
    }

    public string PickupId
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
                _viewModel.PickupId = guid;
        }
    }

    public string DeliveryId
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
                _viewModel.DeliveryId = guid;
        }
    }

    public OrderDetailPage(OrderDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
