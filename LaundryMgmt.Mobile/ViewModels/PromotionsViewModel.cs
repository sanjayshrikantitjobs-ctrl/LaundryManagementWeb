using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class PromotionRow : ObservableObject
{
    public ActivePromotionDto Promotion { get; }

    [ObservableProperty] private bool showCopied;

    public string ButtonLabel => ShowCopied ? "Copied!" : "Use code";

    public PromotionRow(ActivePromotionDto promotion) => Promotion = promotion;

    partial void OnShowCopiedChanged(bool value) => OnPropertyChanged(nameof(ButtonLabel));
}

public partial class PromotionsViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<PromotionRow> Promotions { get; } = new();

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;

    public PromotionsViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var promotions = await _apiClient.GetActivePromotionsAsync();
            Promotions.Clear();
            foreach (var promo in promotions ?? new List<ActivePromotionDto>())
                Promotions.Add(new PromotionRow(promo));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load promotions: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UseCodeAsync(PromotionRow? row)
    {
        if (row is null || string.IsNullOrEmpty(row.Promotion.Code)) return;

        await Clipboard.Default.SetTextAsync(row.Promotion.Code);
        row.ShowCopied = true;
        await Task.Delay(1500);
        row.ShowCopied = false;
    }
}
