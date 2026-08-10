using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public enum SettingsTab { General, Address }

public partial class SettingsViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<CustomerAddressDto> Addresses { get; } = new();

    [ObservableProperty] private SettingsTab activeTab = SettingsTab.General;
    public bool IsGeneralTab => ActiveTab == SettingsTab.General;
    public bool IsAddressTab => ActiveTab == SettingsTab.Address;

    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string? email;
    [ObservableProperty] private string? whatsAppNumber;

    [ObservableProperty] private bool showAddAddressForm;
    [ObservableProperty] private string newAddressLabel = string.Empty;
    [ObservableProperty] private string newAddressLine1 = string.Empty;
    [ObservableProperty] private string? newAddressLine2;
    [ObservableProperty] private string newAddressCity = string.Empty;
    [ObservableProperty] private string newAddressState = string.Empty;
    [ObservableProperty] private string newAddressPostalCode = string.Empty;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string? statusMessage;

    public SettingsViewModel(ApiClient apiClient) => _apiClient = apiClient;

    partial void OnActiveTabChanged(SettingsTab value)
    {
        OnPropertyChanged(nameof(IsGeneralTab));
        OnPropertyChanged(nameof(IsAddressTab));
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var profile = await _apiClient.GetMyCustomerProfileAsync();
            if (profile is not null)
            {
                FullName = profile.FullName;
                Email = profile.Email;
                WhatsAppNumber = profile.WhatsAppNumber;
            }

            await ReloadAddressesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load your settings: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ReloadAddressesAsync()
    {
        var addresses = await _apiClient.GetMyAddressesAsync();
        Addresses.Clear();
        foreach (var address in addresses ?? new List<CustomerAddressDto>())
            Addresses.Add(address);
    }

    [RelayCommand]
    private void SetTab(SettingsTab tab) => ActiveTab = tab;

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            ErrorMessage = "Full name is required.";
            return;
        }

        IsSaving = true;
        ErrorMessage = null;
        StatusMessage = null;

        try
        {
            var response = await _apiClient.UpdateMyProfileAsync(new UpdateMyProfileRequest(FullName, Email, WhatsAppNumber));
            if (response.IsSuccessStatusCode)
                StatusMessage = "Profile updated.";
            else
                ErrorMessage = "Couldn't update your profile.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't update your profile: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void ToggleAddAddressForm() => ShowAddAddressForm = !ShowAddAddressForm;

    [RelayCommand]
    private async Task AddAddressAsync()
    {
        if (string.IsNullOrWhiteSpace(NewAddressLabel) || string.IsNullOrWhiteSpace(NewAddressLine1) ||
            string.IsNullOrWhiteSpace(NewAddressCity) || string.IsNullOrWhiteSpace(NewAddressState) ||
            string.IsNullOrWhiteSpace(NewAddressPostalCode))
        {
            ErrorMessage = "Please fill in all required address fields.";
            return;
        }

        try
        {
            var isFirst = Addresses.Count == 0;
            var response = await _apiClient.AddMyAddressAsync(new CreateCustomerAddressRequest(
                NewAddressLabel, NewAddressLine1, NewAddressLine2, NewAddressCity, NewAddressState, NewAddressPostalCode, isFirst));

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Couldn't save the address.";
                return;
            }

            NewAddressLabel = string.Empty;
            NewAddressLine1 = string.Empty;
            NewAddressLine2 = null;
            NewAddressCity = string.Empty;
            NewAddressState = string.Empty;
            NewAddressPostalCode = string.Empty;
            ShowAddAddressForm = false;

            await ReloadAddressesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't save the address: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SetPrimaryAsync(CustomerAddressDto? address)
    {
        if (address is null) return;

        try
        {
            await _apiClient.SetPrimaryAddressAsync(address.Id);
            await ReloadAddressesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't update the address: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteAddressAsync(CustomerAddressDto? address)
    {
        if (address is null) return;

        try
        {
            await _apiClient.DeleteMyAddressAsync(address.Id);
            await ReloadAddressesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't delete the address: {ex.Message}";
        }
    }
}
