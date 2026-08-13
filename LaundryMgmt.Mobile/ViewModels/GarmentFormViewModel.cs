using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class GarmentFormViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    // Set once categories load and again once the garment loads (edit mode) — whichever
    // finishes last is responsible for resolving SelectedCategory, since Picker binding
    // needs an item reference that's actually in Categories, not just a bare Guid.
    private Guid? _pendingCategoryId;

    public ObservableCollection<ServiceCategoryDto> Categories { get; } = new();

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private ServiceCategoryDto? selectedCategory;
    [ObservableProperty] private string? specialInstructions;
    [ObservableProperty] private string? imageUrl;
    [ObservableProperty] private bool isUploadingImage;
    [ObservableProperty] private bool isEditMode;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private Guid? garmentId;

    public GarmentFormViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        _ = LoadCategoriesAsync();
    }

    partial void OnGarmentIdChanged(Guid? value)
    {
        if (value is Guid id)
            _ = LoadAsync(id);
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _apiClient.GetServiceCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories ?? new List<ServiceCategoryDto>())
                Categories.Add(category);

            if (_pendingCategoryId is Guid pendingId)
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == pendingId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load categories: {ex.Message}";
        }
    }

    private async Task LoadAsync(Guid id)
    {
        IsEditMode = true;

        try
        {
            var garment = await _apiClient.GetGarmentByIdAsync(id);
            if (garment is null) return;

            Name = garment.Name;
            SpecialInstructions = garment.SpecialInstructions;
            ImageUrl = garment.ImageUrl;

            _pendingCategoryId = garment.CategoryId;
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == garment.CategoryId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load the garment: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        var photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 });
        var photo = photos?.FirstOrDefault();
        if (photo is null) return;

        IsUploadingImage = true;
        ErrorMessage = null;
        try
        {
            await using var stream = await photo.OpenReadAsync();
            var uploaded = await _apiClient.UploadImageAsync(stream, photo.FileName, photo.ContentType ?? "image/jpeg");
            if (uploaded is null)
                ErrorMessage = "Image upload failed.";
            else
                ImageUrl = uploaded.Url;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Image upload failed: {ex.Message}";
        }
        finally
        {
            IsUploadingImage = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || SelectedCategory is null)
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
                    Name, SelectedCategory.Id, SpecialInstructions, ImageUrl))
                : await _apiClient.CreateGarmentAsync(
                    new CreateGarmentRequest(Name, SelectedCategory.Id, SpecialInstructions));

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
