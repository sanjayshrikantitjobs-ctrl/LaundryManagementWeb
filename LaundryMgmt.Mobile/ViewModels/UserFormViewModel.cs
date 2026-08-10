using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

/// <summary>Unlike the other *FormViewModels (create-only), this one also edits —
/// UpdateUser/AssignRole both exist server-side and a Users screen without edit
/// would be pointless.</summary>
public partial class UserFormViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public UserRole[] Roles { get; } = StaffRoles.All;

    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string userName = string.Empty;
    [ObservableProperty] private string? email;
    [ObservableProperty] private string? phoneNumber;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private UserRole role = UserRole.Staff;
    [ObservableProperty] private bool isEditMode;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string? errorMessage;

    [ObservableProperty] private Guid? userId;

    private UserRole? _originalRole;

    public UserFormViewModel(ApiClient apiClient) => _apiClient = apiClient;

    partial void OnUserIdChanged(Guid? value)
    {
        if (value is Guid id)
            _ = LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        IsEditMode = true;
        try
        {
            var user = await _apiClient.GetUserByIdAsync(id);
            if (user is null) return;

            FullName = user.FullName;
            UserName = user.UserName;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
            Role = user.Role;
            _originalRole = user.Role;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load user: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Full name, username and email are required.";
            return;
        }

        if (!IsEditMode && Password.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters.";
            return;
        }

        IsSaving = true;
        ErrorMessage = null;

        try
        {
            HttpResponseMessage response;
            if (IsEditMode && UserId is Guid id)
            {
                response = await _apiClient.UpdateUserAsync(id, new UpdateUserRequest(FullName, Email, PhoneNumber));
                if (response.IsSuccessStatusCode && _originalRole != Role)
                    response = await _apiClient.AssignRoleAsync(id, Role);
            }
            else
            {
                response = await _apiClient.CreateUserAsync(new CreateUserRequest(FullName, UserName, Email, PhoneNumber, Password, Role));
            }

            if (response.IsSuccessStatusCode)
                await Shell.Current.GoToAsync("..");
            else
                ErrorMessage = "Failed to save user. The username/email may already be in use.";
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
