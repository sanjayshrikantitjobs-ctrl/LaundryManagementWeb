using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<NotificationDto> Notifications { get; } = new();

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;

    public NotificationsViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var result = await _apiClient.GetMyNotificationsAsync();
            Notifications.Clear();
            foreach (var n in result ?? new List<NotificationDto>())
                Notifications.Add(n);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load notifications: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Mirrors client-web's notification-bell open(): mark read, then jump to whatever
    // the notification is about — a new customer registration goes to that customer's
    // profile, everything else (new/updated order) goes to the order.
    [RelayCommand]
    private async Task OpenAsync(NotificationDto? notification)
    {
        if (notification is null) return;

        if (!notification.IsRead)
        {
            await _apiClient.MarkNotificationReadAsync(notification.Id);
            var index = Notifications.IndexOf(notification);
            if (index >= 0)
                Notifications[index] = notification with { IsRead = true };
        }

        if (string.IsNullOrEmpty(notification.EntityId)) return;

        var route = notification.Type == NotificationType.NewCustomerRegistered
            ? $"{nameof(Views.CustomerDetailPage)}?customerId={notification.EntityId}"
            : $"{nameof(Views.OrderDetailPage)}?orderId={notification.EntityId}";

        await Shell.Current.GoToAsync(route);
    }
}
