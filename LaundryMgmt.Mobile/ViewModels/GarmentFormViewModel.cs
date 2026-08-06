using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class GarmentFormViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string category = string.Empty;
    [ObservableProperty] private string? barcode;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string? errorMessage;

    public GarmentFormViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Category))
        {
            ErrorMessage = "Name and category are required.";
            return;
        }

        IsSaving = true;
        ErrorMessage = null;

        try
        {
            var response = await _apiClient.CreateGarmentAsync(
                new CreateGarmentRequest(Name, Category, string.IsNullOrWhiteSpace(Barcode) ? null : Barcode, null));

            if (response.IsSuccessStatusCode)
                await Shell.Current.GoToAsync("..");
            else
                ErrorMessage = "Failed to save garment.";
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
