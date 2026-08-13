using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

[QueryProperty(nameof(UserId), "userId")]
public partial class UserDetailPage : ContentPage
{
    private readonly UserDetailViewModel _viewModel;

    public string UserId
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
                _viewModel.UserId = guid;
        }
    }

    public UserDetailPage(UserDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
