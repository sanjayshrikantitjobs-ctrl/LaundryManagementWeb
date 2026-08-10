using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class GarmentFormViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    // Not shown on this form but must round-trip unchanged on Update — the API
    // assigns request.ImageUrl/SpecialInstructions directly onto the entity, so
    // sending null here would silently wipe an existing catalog photo/notes.
    private string? _existingSpecialInstructions;
    private string? _existingImageUrl;

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string category = string.Empty;
    [ObservableProperty] private string? barcode;
    [ObservableProperty] private bool isEditMode;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private Guid? garmentId;

    public GarmentFormViewModel(ApiClient apiClient) => _apiClient = apiClient;

    partial void OnGarmentIdChanged(Guid? value)
    {
        if (value is Guid id)
            _ = LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        IsEditMode = true;

        try
        {
            var garment = await _apiClient.GetGarmentByIdAsync(id);
            if (garment is null) return;

            Name = garment.Name;
            Category = garment.Category;
            Barcode = garment.Barcode;
            _existingSpecialInstructions = garment.SpecialInstructions;
            _existingImageUrl = garment.ImageUrl;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load the garment: {ex.Message}";
        }
    }

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
            var response = IsEditMode && GarmentId is Guid id
                ? await _apiClient.UpdateGarmentAsync(id, new UpdateGarmentRequest(
                    Name, Category, string.IsNullOrWhiteSpace(Barcode) ? null : Barcode,
                    _existingSpecialInstructions, _existingImageUrl))
                : await _apiClient.CreateGarmentAsync(
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
