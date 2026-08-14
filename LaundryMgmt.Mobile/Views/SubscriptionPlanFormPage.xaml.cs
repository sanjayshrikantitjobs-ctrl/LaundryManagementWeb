using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

[QueryProperty(nameof(PlanId), "planId")]
public partial class SubscriptionPlanFormPage : ContentPage
{
    private readonly SubscriptionPlanFormViewModel _viewModel;

    public string PlanId
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
                _viewModel.PlanId = guid;
        }
    }

    public SubscriptionPlanFormPage(SubscriptionPlanFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
