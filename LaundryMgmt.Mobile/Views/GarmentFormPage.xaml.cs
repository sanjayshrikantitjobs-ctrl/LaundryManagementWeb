using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

public partial class GarmentFormPage : ContentPage
{
    public GarmentFormPage(GarmentFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
