using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

public partial class OrderQueuePage : ContentPage
{
    public OrderQueuePage(OrderQueueViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
