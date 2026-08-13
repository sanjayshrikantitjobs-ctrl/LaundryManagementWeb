using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

[QueryProperty(nameof(ServiceId), "serviceId")]
public partial class ServiceDetailPage : ContentPage
{
    private readonly ServiceDetailViewModel _viewModel;

    public string ServiceId
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
                _viewModel.ServiceId = guid;
        }
    }

    public ServiceDetailPage(ServiceDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
