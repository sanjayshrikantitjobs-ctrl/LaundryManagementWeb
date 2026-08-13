using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class ServiceFormViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    // Set once categories load and again once the service loads (edit mode) — whichever
    // finishes last resolves SelectedCategory, matching GarmentFormViewModel's pattern.
    private Guid? _pendingCategoryId;

    public ObservableCollection<ServiceCategoryDto> Categories { get; } = new();

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private ServiceCategoryDto? selectedCategory;
    [ObservableProperty] private string basePriceText = "0";
    [ObservableProperty] private string estimatedTimeHoursText = "1";
    [ObservableProperty] private string gstPercentageText = "5";
    [ObservableProperty] private string priorityText = "0";
    [ObservableProperty] private string expressSurchargeText = "0";
    [ObservableProperty] private string expressEtaHoursText = "24";
    [ObservableProperty] private string? imageUrl;
    [ObservableProperty] private bool isUploadingImage;
    [ObservableProperty] private bool isEditMode;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private Guid? serviceId;

    public ServiceFormViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        _ = LoadCategoriesAsync();
    }

    partial void OnServiceIdChanged(Guid? value)
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
            var service = await _apiClient.GetServiceByIdAsync(id);
            if (service is null) return;

            Name = service.Name;
            BasePriceText = service.BasePrice.ToString("0.##");
            EstimatedTimeHoursText = service.EstimatedTimeHours.ToString();
            GstPercentageText = service.GstPercentage.ToString("0.##");
            PriorityText = service.Priority.ToString();
            ExpressSurchargeText = service.ExpressSurcharge.ToString("0.##");
            ExpressEtaHoursText = service.ExpressEtaHours.ToString();
            ImageUrl = service.ImageUrl;

            _pendingCategoryId = service.CategoryId;
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == service.CategoryId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load the service: {ex.Message}";
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

        decimal.TryParse(BasePriceText, out var basePrice);
        int.TryParse(EstimatedTimeHoursText, out var estimatedTimeHours);
        decimal.TryParse(GstPercentageText, out var gstPercentage);
        int.TryParse(PriorityText, out var priority);
        decimal.TryParse(ExpressSurchargeText, out var expressSurcharge);
        int.TryParse(ExpressEtaHoursText, out var expressEtaHours);

        IsSaving = true;
        ErrorMessage = null;

        try
        {
            var response = IsEditMode && ServiceId is Guid id
                ? await _apiClient.UpdateServiceAsync(id, new UpdateServiceRequest(
                    Name, SelectedCategory.Id, basePrice, estimatedTimeHours, gstPercentage, priority,
                    ImageUrl, expressSurcharge, expressEtaHours))
                : await _apiClient.CreateServiceAsync(new CreateServiceRequest(
                    Name, SelectedCategory.Id, basePrice, estimatedTimeHours, gstPercentage, priority,
                    ImageUrl, expressSurcharge, expressEtaHours));

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
