using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

[QueryProperty(nameof(ServiceId), "serviceId")]
public partial class ServiceFormPage : ContentPage
{
    private readonly ServiceFormViewModel _viewModel;

    public string ServiceId
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
                _viewModel.ServiceId = guid;
        }
    }

    public ServiceFormPage(ServiceFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
