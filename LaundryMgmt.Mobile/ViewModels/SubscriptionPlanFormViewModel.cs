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
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string? errorMessage;

    public SubscriptionPlanFormViewModel(ApiClient apiClient) => _apiClient = apiClient;

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
            var response = await _apiClient.CreateSubscriptionPlanAsync(new CreateSubscriptionPlanRequest(
                Name, string.IsNullOrWhiteSpace(Description) ? null : Description, BillingCycle,
                garmentsPerCycle, price, 0, features));

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
}
