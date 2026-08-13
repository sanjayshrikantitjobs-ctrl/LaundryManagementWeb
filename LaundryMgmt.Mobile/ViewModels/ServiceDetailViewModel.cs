using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class ServiceDetailViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    [ObservableProperty] private Guid? serviceId;
    [ObservableProperty] private ServiceDetailDto? service;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;

    public bool CanEditMasterData => _authService.Role is not ("Customer" or "DepartmentHead");

    public ServiceDetailViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    partial void OnServiceIdChanged(Guid? value)
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
            Service = await _apiClient.GetServiceByIdAsync(id);
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
        if (Service is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.ServiceFormPage)}?serviceId={Service.Id}");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Service is null) return;

        var confirmed = await Shell.Current.DisplayAlert(
            "Delete service", $"Delete \"{Service.Name}\"? This cannot be undone.", "Continue", "Cancel");
        if (!confirmed) return;

        var reason = await Shell.Current.DisplayPromptAsync(
            "Reason for deletion", "Why is this service being deleted?", "Delete", "Cancel", placeholder: "Reason (optional)");
        if (reason is null) return; // cancelled

        await _apiClient.DeleteServiceAsync(Service.Id, reason);
        await Shell.Current.GoToAsync("..");
    }
}
