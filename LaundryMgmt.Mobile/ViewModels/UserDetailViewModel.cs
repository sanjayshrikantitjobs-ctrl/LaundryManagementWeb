using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class UserDetailViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    [ObservableProperty] private Guid? userId;
    [ObservableProperty] private UserSummaryDto? user;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;

    public UserDetailViewModel(ApiClient apiClient) => _apiClient = apiClient;

    partial void OnUserIdChanged(Guid? value)
    {
        if (value is Guid id)
            _ = LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            User = await _apiClient.GetUserByIdAsync(id);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (User is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.UserFormPage)}?userId={User.Id}");
    }

    [RelayCommand]
    private async Task ToggleActiveAsync()
    {
        if (User is null) return;

        var action = User.IsActive ? "Deactivate" : "Activate";
        var confirmed = await Shell.Current.DisplayAlert($"{action} user", $"{action} \"{User.FullName}\"?", action, "Cancel");
        if (!confirmed) return;

        await _apiClient.SetUserActiveAsync(User.Id, !User.IsActive);
        await LoadAsync(User.Id);
    }
}
