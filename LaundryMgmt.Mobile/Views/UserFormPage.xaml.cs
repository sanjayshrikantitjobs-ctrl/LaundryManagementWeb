using LaundryMgmt.Mobile.ViewModels;

namespace LaundryMgmt.Mobile.Views;

[QueryProperty(nameof(UserId), "userId")]
public partial class UserFormPage : ContentPage
{
    private readonly UserFormViewModel _viewModel;

    public string UserId
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
                _viewModel.UserId = guid;
        }
    }

    public UserFormPage(UserFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
