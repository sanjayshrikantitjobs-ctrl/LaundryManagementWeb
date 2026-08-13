using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

[QueryProperty(nameof(GarmentId), "garmentId")]
public partial class GarmentDetailPage : ContentPage
{
    private readonly GarmentDetailViewModel _viewModel;

    public string GarmentId
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
                _viewModel.GarmentId = guid;
        }
    }

    public GarmentDetailPage(GarmentDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
