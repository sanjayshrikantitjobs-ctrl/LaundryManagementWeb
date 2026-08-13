using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class GarmentDetailViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    [ObservableProperty] private Guid? garmentId;
    [ObservableProperty] private GarmentDetailDto? garment;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;

    public bool CanEditMasterData => _authService.Role is not ("Customer" or "DepartmentHead");

    public GarmentDetailViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    partial void OnGarmentIdChanged(Guid? value)
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
            Garment = await _apiClient.GetGarmentByIdAsync(id);
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
        if (Garment is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.GarmentFormPage)}?garmentId={Garment.Id}");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Garment is null) return;

        var confirmed = await Shell.Current.DisplayAlert(
            "Delete garment", $"Delete \"{Garment.Name}\"? This cannot be undone.", "Continue", "Cancel");
        if (!confirmed) return;

        var reason = await Shell.Current.DisplayPromptAsync(
            "Reason for deletion", "Why is this garment being deleted?", "Delete", "Cancel", placeholder: "Reason (optional)");
        if (reason is null) return; // cancelled

        await _apiClient.DeleteGarmentAsync(Garment.Id, reason);
        await Shell.Current.GoToAsync("..");
    }
}
