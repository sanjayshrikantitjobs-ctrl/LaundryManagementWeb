using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;

namespace LaundryMgmt.Mobile.ViewModels.Paging;

/// <summary>Base for list ViewModels backed by a paginated API endpoint. Layers
/// "Load More" (driven by CollectionView's RemainingItemsThreshold) on top of the
/// existing pull-to-refresh convention (RefreshCommand) — subclasses only implement
/// FetchPageAsync and get paging, refresh, and load-more behavior for free.</summary>
public abstract partial class PagedListViewModel<TItem> : ObservableObject
{
    private const int DefaultPageSize = 20;

    public ObservableCollection<TItem> Items { get; } = new();

    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private string? errorMessage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadMoreCommand))]
    private bool isLoadingMore;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadMoreCommand))]
    private bool hasMore;

    private int _pageNumber = 1;

    protected virtual int PageSize => DefaultPageSize;

    /// <summary>Fetches one page from whichever ApiClient endpoint the subclass wraps.</summary>
    protected abstract Task<PaginatedList<TItem>?> FetchPageAsync(int pageNumber, int pageSize);

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsRefreshing = true;
        ErrorMessage = null;
        _pageNumber = 1;
        try
        {
            var result = await FetchPageAsync(_pageNumber, PageSize);
            Items.Clear();
            foreach (var item in result?.Items ?? new List<TItem>())
                Items.Add(item);
            HasMore = result?.HasNextPage ?? false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoadMore))]
    public async Task LoadMoreAsync()
    {
        IsLoadingMore = true;
        try
        {
            var nextPage = _pageNumber + 1;
            var result = await FetchPageAsync(nextPage, PageSize);
            foreach (var item in result?.Items ?? new List<TItem>())
                Items.Add(item);
            _pageNumber = nextPage;
            HasMore = result?.HasNextPage ?? false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load more: {ex.Message}";
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    private bool CanLoadMore() => HasMore && !IsLoadingMore;
}
