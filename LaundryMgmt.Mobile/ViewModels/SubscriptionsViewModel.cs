using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class SubscriptionsViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public ObservableCollection<SubscriptionPlanDto> Plans { get; } = new();

    public bool CanEditMasterData => _authService.Role is not ("Customer" or "DepartmentHead");

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;

    public SubscriptionsViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var plans = await _apiClient.GetSubscriptionPlansAsync();
            Plans.Clear();
            foreach (var plan in (plans ?? new List<SubscriptionPlanDto>()).Where(p => p.IsActive || CanEditMasterData))
                Plans.Add(plan);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load subscription plans: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NewPlanAsync() => await Shell.Current.GoToAsync(nameof(Views.SubscriptionPlanFormPage));

    /// <summary>Row tap opens the plan for editing (and, from there, deleting) — replacing
    /// the old inline per-row Delete button, matching the Customers list's
    /// tap-to-open-detail pattern (see CustomerDetailViewModel).</summary>
    [RelayCommand]
    private async Task OpenPlanAsync(SubscriptionPlanDto? plan)
    {
        if (plan is null || !CanEditMasterData) return;
        await Shell.Current.GoToAsync($"{nameof(Views.SubscriptionPlanFormPage)}?planId={plan.Id}");
    }
}
