using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels.Paging;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class GarmentsViewModel : PagedListViewModel<GarmentListItem>
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    /// <summary>Synthetic entry prepended to <see cref="Categories"/> so the Picker always
    /// has an "All Categories" option to fall back to — MAUI's Picker has no built-in way
    /// to clear a selection back to null once something's been picked.</summary>
    private static readonly ServiceCategoryDto AllCategories = new(Guid.Empty, "All Categories", null, null, -1, true);

    public ObservableCollection<GarmentListItem> Garments => Items;

    // Must already contain AllCategories at construction, matching SelectedCategory's
    // initial value below — the Picker's SelectedItem is two-way bound, and if the bound
    // object isn't present in ItemsSource at bind time, MAUI resets SelectedItem to null,
    // which then NullReferenceExceptions in FetchPageAsync's SelectedCategory.Id access.
    public ObservableCollection<ServiceCategoryDto> Categories { get; } = new() { AllCategories };

    [ObservableProperty] private ServiceCategoryDto selectedCategory = AllCategories;

    /// <summary>Garments create/edit/delete is master-data management — everyone
    /// except Customer and DepartmentHead (matches API AppRoles.ManagementRoles).</summary>
    public bool CanEditMasterData => _authService.Role is not ("Customer" or "DepartmentHead");

    public GarmentsViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
        _ = LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        // AllCategories is already the first (and only) entry from construction — append
        // rather than Clear()+re-add, since a transient empty ItemsSource mid-reload would
        // hit the same SelectedItem-desync issue documented on the Categories property.
        var categories = await _apiClient.GetServiceCategoriesAsync();
        foreach (var category in categories ?? new List<ServiceCategoryDto>())
            Categories.Add(category);
    }

    partial void OnSelectedCategoryChanged(ServiceCategoryDto value) => _ = RefreshAsync();

    protected override Task<PaginatedList<GarmentListItem>?> FetchPageAsync(int pageNumber, int pageSize) =>
        _apiClient.GetGarmentsAsync(
            categoryId: SelectedCategory.Id == Guid.Empty ? null : SelectedCategory.Id,
            pageNumber: pageNumber, pageSize: pageSize);

    [RelayCommand]
    private async Task NewGarmentAsync() => await Shell.Current.GoToAsync(nameof(Views.GarmentFormPage));

    /// <summary>Row tap opens the read-only detail page (with the real photo), where
    /// Edit/Delete now live — see GarmentDetailViewModel.</summary>
    [RelayCommand]
    private async Task OpenGarmentAsync(GarmentListItem? garment)
    {
        if (garment is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.GarmentDetailPage)}?garmentId={garment.Id}");
    }
}
