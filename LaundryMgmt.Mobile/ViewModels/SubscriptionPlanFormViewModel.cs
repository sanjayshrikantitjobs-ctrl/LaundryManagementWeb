using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class SubscriptionPlanFormViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public BillingCycle[] BillingCycles { get; } = Enum.GetValues<BillingCycle>();

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private BillingCycle billingCycle = BillingCycle.Monthly;
    [ObservableProperty] private string garmentsPerCycleText = "20";
    [ObservableProperty] private string priceText = "0";
    [ObservableProperty] private string featuresText = string.Empty;
    [ObservableProperty] private bool isActive = true;
    [ObservableProperty] private bool isEditMode;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string? errorMessage;

    [ObservableProperty] private Guid? planId;

    public SubscriptionPlanFormViewModel(ApiClient apiClient) => _apiClient = apiClient;

    partial void OnPlanIdChanged(Guid? value)
    {
        if (value is Guid id)
            _ = LoadAsync(id);
    }

    // Subscription plans have no single-item GET endpoint (only the full list) — the
    // list is short enough (a handful of plans) that fetching it all and filtering
    // client-side is simpler than adding a new API endpoint just for this.
    private async Task LoadAsync(Guid id)
    {
        IsEditMode = true;
        try
        {
            var plans = await _apiClient.GetSubscriptionPlansAsync();
            var plan = plans?.FirstOrDefault(p => p.Id == id);
            if (plan is null) return;

            Name = plan.Name;
            Description = plan.Description ?? string.Empty;
            BillingCycle = plan.BillingCycle;
            GarmentsPerCycleText = plan.GarmentsPerCycle.ToString();
            PriceText = plan.Price.ToString("0.##");
            FeaturesText = string.Join('\n', plan.Features.OrderBy(f => f.DisplayOrder).Select(f => f.Text));
            IsActive = plan.IsActive;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load plan: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Name is required.";
            return;
        }

        int.TryParse(GarmentsPerCycleText, out var garmentsPerCycle);
        decimal.TryParse(PriceText, out var price);
        var features = FeaturesText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        IsSaving = true;
        ErrorMessage = null;

        try
        {
            HttpResponseMessage response;
            if (IsEditMode && PlanId is Guid id)
            {
                response = await _apiClient.UpdateSubscriptionPlanAsync(id, new UpdateSubscriptionPlanRequest(
                    Name, string.IsNullOrWhiteSpace(Description) ? null : Description, BillingCycle,
                    garmentsPerCycle, price, 0, IsActive, features));
            }
            else
            {
                response = await _apiClient.CreateSubscriptionPlanAsync(new CreateSubscriptionPlanRequest(
                    Name, string.IsNullOrWhiteSpace(Description) ? null : Description, BillingCycle,
                    garmentsPerCycle, price, 0, features));
            }

            if (response.IsSuccessStatusCode)
                await Shell.Current.GoToAsync("..");
            else
                ErrorMessage = "Failed to save plan.";
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

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (PlanId is not Guid id) return;

        var confirmed = await Shell.Current.DisplayAlert("Delete plan", $"Delete \"{Name}\"?", "Delete", "Cancel");
        if (!confirmed) return;

        var response = await _apiClient.DeleteSubscriptionPlanAsync(id);
        if (response.IsSuccessStatusCode)
            await Shell.Current.GoToAsync("..");
        else
            ErrorMessage = "Couldn't delete this plan — it may still have active subscribers.";
    }
}
