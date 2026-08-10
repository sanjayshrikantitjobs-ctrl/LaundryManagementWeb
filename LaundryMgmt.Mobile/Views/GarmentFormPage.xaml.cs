using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

[QueryProperty(nameof(GarmentId), "garmentId")]
public partial class GarmentFormPage : ContentPage
{
    private readonly GarmentFormViewModel _viewModel;

    public string GarmentId
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
                _viewModel.GarmentId = guid;
        }
    }

    public GarmentFormPage(GarmentFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
