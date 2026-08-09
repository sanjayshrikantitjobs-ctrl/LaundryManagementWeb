using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class ServicesViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<ServiceListItem> Services { get; } = new();

    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private string? errorMessage;

    public ServicesViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsRefreshing = true;
        ErrorMessage = null;
        try
        {
            var result = await _apiClient.GetServicesAsync();
            Services.Clear();
            foreach (var service in result?.Items ?? new List<ServiceListItem>())
                Services.Add(service);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load services: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task NewServiceAsync() => await Shell.Current.GoToAsync(nameof(Views.ServiceFormPage));

    [RelayCommand]
    private async Task DeleteServiceAsync(ServiceListItem? service)
    {
        if (service is null) return;
        var confirmed = await Shell.Current.DisplayAlert("Delete service", $"Delete \"{service.Name}\"?", "Delete", "Cancel");
        if (!confirmed) return;

        await _apiClient.DeleteServiceAsync(service.Id);
        await RefreshAsync();
    }
}
