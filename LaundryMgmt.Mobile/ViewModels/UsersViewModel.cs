using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;
using LaundryMgmt.Mobile.ViewModels.Paging;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class UsersViewModel : PagedListViewModel<UserSummaryDto>
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<UserSummaryDto> Users => Items;

    [ObservableProperty] private string searchText = string.Empty;

    public UsersViewModel(ApiClient apiClient) => _apiClient = apiClient;

    protected override Task<PaginatedList<UserSummaryDto>?> FetchPageAsync(int pageNumber, int pageSize) =>
        _apiClient.GetUsersAsync(search: SearchText, pageNumber: pageNumber, pageSize: pageSize);

    [RelayCommand]
    private async Task NewUserAsync() => await Shell.Current.GoToAsync(nameof(Views.UserFormPage));

    /// <summary>Row tap opens the read-only detail page, where Edit/Deactivate now
    /// live (replacing the old inline per-row buttons — see UserDetailViewModel).</summary>
    [RelayCommand]
    private async Task OpenUserAsync(UserSummaryDto? user)
    {
        if (user is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.UserDetailPage)}?userId={user.Id}");
    }
}
