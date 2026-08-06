using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class ServiceFormViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string basePriceText = "0";
    [ObservableProperty] private string estimatedTimeHoursText = "1";
    [ObservableProperty] private string gstPercentageText = "5";
    [ObservableProperty] private string priorityText = "0";
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string? errorMessage;

    public ServiceFormViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Name is required.";
            return;
        }

        decimal.TryParse(BasePriceText, out var basePrice);
        int.TryParse(EstimatedTimeHoursText, out var estimatedTimeHours);
        decimal.TryParse(GstPercentageText, out var gstPercentage);
        int.TryParse(PriorityText, out var priority);

        IsSaving = true;
        ErrorMessage = null;

        try
        {
            var response = await _apiClient.CreateServiceAsync(
                new CreateServiceRequest(Name, basePrice, estimatedTimeHours, gstPercentage, priority));

            if (response.IsSuccessStatusCode)
                await Shell.Current.GoToAsync("..");
            else
                ErrorMessage = "Failed to save service.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't reach the server: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }
}
