using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels.Paging;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class GarmentsViewModel : PagedListViewModel<GarmentListItem>
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public ObservableCollection<GarmentListItem> Garments => Items;

    /// <summary>Garments create/edit/delete is master-data management — everyone
    /// except Customer and DepartmentHead (matches API AppRoles.ManagementRoles).</summary>
    public bool CanEditMasterData => _authService.Role is not ("Customer" or "DepartmentHead");

    public GarmentsViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    protected override Task<PaginatedList<GarmentListItem>?> FetchPageAsync(int pageNumber, int pageSize) =>
        _apiClient.GetGarmentsAsync(pageNumber: pageNumber, pageSize: pageSize);

    [RelayCommand]
    private async Task NewGarmentAsync() => await Shell.Current.GoToAsync(nameof(Views.GarmentFormPage));

    [RelayCommand]
    private async Task EditGarmentAsync(GarmentListItem? garment)
    {
        if (garment is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.GarmentFormPage)}?garmentId={garment.Id}");
    }

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
