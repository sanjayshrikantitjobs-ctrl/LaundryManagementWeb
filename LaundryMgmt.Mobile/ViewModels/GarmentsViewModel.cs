using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class GarmentsViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<GarmentListItem> Garments { get; } = new();

    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private string? errorMessage;

    public GarmentsViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsRefreshing = true;
        ErrorMessage = null;
        try
        {
            var result = await _apiClient.GetGarmentsAsync();
            Garments.Clear();
            foreach (var garment in result?.Items ?? new List<GarmentListItem>())
                Garments.Add(garment);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load garments: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task NewGarmentAsync() => await Shell.Current.GoToAsync(nameof(Views.GarmentFormPage));

    [RelayCommand]
    private async Task DeleteGarmentAsync(GarmentListItem? garment)
    {
        if (garment is null) return;
        var confirmed = await Shell.Current.DisplayAlert("Delete garment", $"Delete \"{garment.Name}\"?", "Delete", "Cancel");
        if (!confirmed) return;

        await _apiClient.DeleteGarmentAsync(garment.Id);
        await RefreshAsync();
    }
}
