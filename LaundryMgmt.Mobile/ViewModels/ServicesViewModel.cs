using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels.Paging;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class ServicesViewModel : PagedListViewModel<ServiceListItem>
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public ObservableCollection<ServiceListItem> Services => Items;

    public bool CanEditMasterData => _authService.Role is not ("Customer" or "DepartmentHead");

    public ServicesViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    protected override Task<PaginatedList<ServiceListItem>?> FetchPageAsync(int pageNumber, int pageSize) =>
        _apiClient.GetServicesAsync(pageNumber: pageNumber, pageSize: pageSize);

    [RelayCommand]
    private async Task NewServiceAsync() => await Shell.Current.GoToAsync(nameof(Views.ServiceFormPage));

    /// <summary>Row tap opens the read-only detail page, where Edit/Delete now live
    /// (replacing the old inline Delete-only button — see ServiceDetailViewModel).</summary>
    [RelayCommand]
    private async Task OpenServiceAsync(ServiceListItem? service)
    {
        if (service is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.ServiceDetailPage)}?serviceId={service.Id}");
    }
}
