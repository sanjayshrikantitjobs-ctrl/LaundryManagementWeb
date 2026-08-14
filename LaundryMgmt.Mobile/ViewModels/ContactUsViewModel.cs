using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class ContactUsViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public ContactMessageType[] MessageTypes { get; } = Enum.GetValues<ContactMessageType>();
    public ObservableCollection<MyContactMessageDto> Messages { get; } = new();

    [ObservableProperty] private ContactMessageType selectedType = ContactMessageType.Feedback;
    [ObservableProperty] private string message = string.Empty;
    [ObservableProperty] private string? photoUrl;

    [ObservableProperty] private bool isUploadingPhoto;
    [ObservableProperty] private bool isSubmitting;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string? statusMessage;

    public const string SupportPhoneNumber = "+919867343302";

    public ContactUsViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [RelayCommand]
    private async Task CallUsAsync()
    {
        try
        {
            await Launcher.Default.OpenAsync(new Uri($"tel:{SupportPhoneNumber}"));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't open the dialer: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var messages = await _apiClient.GetMyContactMessagesAsync();
            Messages.Clear();
            foreach (var item in (messages ?? new List<MyContactMessageDto>()).OrderByDescending(m => m.CreatedAtUtc))
                Messages.Add(item);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load your messages: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        if (!MediaPicker.Default.IsCaptureSupported) return;
        var photo = await MediaPicker.Default.CapturePhotoAsync();
        if (photo is not null)
            await UploadPhotoAsync(photo);
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        var photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 });
        var photo = photos?.FirstOrDefault();
        if (photo is not null)
            await UploadPhotoAsync(photo);
    }

    private async Task UploadPhotoAsync(FileResult photo)
    {
        IsUploadingPhoto = true;
        ErrorMessage = null;

        try
        {
            await using var stream = await photo.OpenReadAsync();
            var uploaded = await _apiClient.UploadImageAsync(stream, photo.FileName, photo.ContentType ?? "image/jpeg");
            PhotoUrl = uploaded?.Url;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't upload the photo: {ex.Message}";
        }
        finally
        {
            IsUploadingPhoto = false;
        }
    }

    [RelayCommand]
    private void RemovePhoto() => PhotoUrl = null;

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Message))
        {
            ErrorMessage = "Please enter a message.";
            return;
        }

        if (Message.Length > 2000)
        {
            ErrorMessage = "Message must be under 2000 characters.";
            return;
        }

        IsSubmitting = true;
        ErrorMessage = null;
        StatusMessage = null;

        try
        {
            var response = await _apiClient.CreateContactMessageAsync(new CreateContactMessageRequest(SelectedType, Message, PhotoUrl));
            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Couldn't send your message.";
                return;
            }

            Message = string.Empty;
            PhotoUrl = null;
            StatusMessage = "Your message has been sent.";
            await InitializeAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't send your message: {ex.Message}";
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}
