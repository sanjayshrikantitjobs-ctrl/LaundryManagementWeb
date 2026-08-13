using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

public partial class SubscriptionPlanFormPage : ContentPage
{
    public SubscriptionPlanFormPage(SubscriptionPlanFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
