using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Mobile.Services;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class ServiceTabItem
{
    public Guid ServiceId { get; init; }
    public string ServiceName { get; init; } = string.Empty;
}

public partial class AssignableLeg : ObservableObject
{
    public OrderPickupDeliveryDto Leg { get; }
    public List<AgentDto> Agents { get; }

    [ObservableProperty] private AgentDto? selectedAgent;

    public AssignableLeg(OrderPickupDeliveryDto leg, List<AgentDto> agents)
    {
        Leg = leg;
        Agents = agents;
        SelectedAgent = agents.FirstOrDefault(a => a.EmployeeId == leg.AssignedEmployeeId);
    }
}

public partial class OrderDetailViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    private List<OrderGarmentImageDto> _allImages = new();

    [ObservableProperty] private OrderDetailDto? order;
    [ObservableProperty] private bool isLoading = true;
    [ObservableProperty] private string? errorMessage;

    [ObservableProperty] private Guid? orderId;
    [ObservableProperty] private Guid? pickupId;
    [ObservableProperty] private Guid? deliveryId;

    [ObservableProperty] private GarmentImageType imageTypeTab = GarmentImageType.Pickup;
    [ObservableProperty] private Guid? serviceFilter;
    [ObservableProperty] private bool isUploading;
    [ObservableProperty] private string? uploadNotes;
    [ObservableProperty] private string? uploadError;

    [ObservableProperty] private bool isConfirming;
    [ObservableProperty] private string? confirmError;

    // Manage Order panel (management roles only — mirrors web's order-detail "Manage
    // Order" card: status/payment status/amount paid/expected delivery, all editable
    // in one PUT rather than the single-field AdvanceOrderStatus endpoint used elsewhere).
    [ObservableProperty] private OrderStatus editStatus;
    [ObservableProperty] private PaymentStatus editPaymentStatus;
    [ObservableProperty] private string editAmountPaidText = "0";
    [ObservableProperty] private DateTime editPromisedDate = DateTime.Today;
    [ObservableProperty] private TimeSpan editPromisedTime;
    [ObservableProperty] private bool isSavingOrder;
    [ObservableProperty] private string? saveOrderError;
    [ObservableProperty] private string? saveOrderStatus;

    public OrderStatus[] OrderStatuses { get; } = Enum.GetValues<OrderStatus>();
    public PaymentStatus[] PaymentStatuses { get; } = Enum.GetValues<PaymentStatus>();

    public ObservableCollection<OrderGarmentImageDto> FilteredImages { get; } = new();
    public ObservableCollection<ServiceTabItem> ServiceTabs { get; } = new();
    public ObservableCollection<AssignableLeg> PickupDeliveries { get; } = new();

    public GarmentImageType[] ImageTypeTabs { get; } = Enum.GetValues<GarmentImageType>();

    private string? Role => _authService.Role;

    /// <summary>Everyone who can view images/pickup-delivery info for any order,
    /// not just their own assignment — matches API AppRoles.OperationalViewRoles.</summary>
    public bool CanViewOperations => Role is "Admin" or "StoreManager" or "Staff" or "DepartmentHead";

    /// <summary>The agent-assignment panel only — excludes DepartmentHead, matching
    /// API AppRoles.ManagementRoles (PickupDeliveryController's assign/agents endpoints).</summary>
    public bool IsAssignManagement => Role is "Admin" or "StoreManager" or "Staff";

    public bool IsPickupAgent => Role == "PickupAgent";
    public bool IsDeliveryAgent => Role == "DeliveryAgent";

    public bool CanUpload
    {
        get
        {
            if (CanViewOperations) return true;
            if (IsPickupAgent) return ImageTypeTab == GarmentImageType.Pickup && PickupId.HasValue;
            if (IsDeliveryAgent) return ImageTypeTab == GarmentImageType.Delivery && DeliveryId.HasValue;
            return false;
        }
    }

    public OrderDetailViewModel(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    public async Task LoadAsync(Guid id)
    {
        OrderId = id;
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Order = await _apiClient.GetOrderByIdAsync(id);
            ApplyOrderToEditFields();
            RebuildServiceTabs();
            await LoadImagesAsync();

            if (CanViewOperations)
                await LoadPickupDeliveriesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load this order: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyOrderToEditFields()
    {
        if (Order is null) return;

        EditStatus = Order.Status;
        EditPaymentStatus = Order.PaymentStatus;
        EditAmountPaidText = Order.AmountPaid.ToString("0.##");

        if (Order.PromisedByUtc is DateTimeOffset promised)
        {
            var local = promised.ToLocalTime();
            EditPromisedDate = local.Date;
            EditPromisedTime = local.TimeOfDay;
        }
    }

    [RelayCommand]
    private async Task SaveOrderAsync()
    {
        if (OrderId is not Guid id) return;

        if (!decimal.TryParse(EditAmountPaidText, out var amountPaid) || amountPaid < 0)
        {
            SaveOrderError = "Enter a valid amount paid.";
            return;
        }

        IsSavingOrder = true;
        SaveOrderError = null;
        SaveOrderStatus = null;

        try
        {
            var promisedByUtc = new DateTimeOffset(EditPromisedDate.Date + EditPromisedTime, DateTimeOffset.Now.Offset);
            var request = new UpdateOrderRequest(EditStatus, EditPaymentStatus, amountPaid, promisedByUtc);
            var response = await _apiClient.UpdateOrderAsync(id, request);

            if (response.IsSuccessStatusCode)
            {
                SaveOrderStatus = "Order updated.";
                await LoadAsync(id);
            }
            else
            {
                SaveOrderError = "Failed to update the order.";
            }
        }
        catch (Exception ex)
        {
            SaveOrderError = $"Couldn't reach the server: {ex.Message}";
        }
        finally
        {
            IsSavingOrder = false;
        }
    }

    private void RebuildServiceTabs()
    {
        ServiceTabs.Clear();
        if (Order is null) return;

        foreach (var item in Order.Items.DistinctBy(i => i.ServiceId))
            ServiceTabs.Add(new ServiceTabItem { ServiceId = item.ServiceId, ServiceName = item.ServiceName });
    }

    private async Task LoadImagesAsync()
    {
        if (OrderId is not Guid id) return;

        var images = await _apiClient.GetOrderImagesAsync(id);
        _allImages = images ?? new List<OrderGarmentImageDto>();
        RebuildFilteredImages();
    }

    private void RebuildFilteredImages()
    {
        FilteredImages.Clear();
        foreach (var image in _allImages.Where(i => i.ImageType == ImageTypeTab && (ServiceFilter is null || i.ServiceId == ServiceFilter)))
            FilteredImages.Add(image);
    }

    private async Task LoadPickupDeliveriesAsync()
    {
        if (OrderId is not Guid id) return;

        var legs = await _apiClient.GetOrderPickupDeliveriesAsync(id) ?? new List<OrderPickupDeliveryDto>();

        List<AgentDto> pickupAgents = new();
        List<AgentDto> deliveryAgents = new();
        if (IsAssignManagement)
        {
            pickupAgents = await _apiClient.GetAgentsAsync(UserRole.PickupAgent) ?? new List<AgentDto>();
            deliveryAgents = await _apiClient.GetAgentsAsync(UserRole.DeliveryAgent) ?? new List<AgentDto>();
        }

        PickupDeliveries.Clear();
        foreach (var leg in legs)
            PickupDeliveries.Add(new AssignableLeg(leg, leg.IsPickup ? pickupAgents : deliveryAgents));
    }

    [RelayCommand]
    private void SetImageTypeTab(GarmentImageType type)
    {
        ImageTypeTab = type;
        RebuildFilteredImages();
    }

    [RelayCommand]
    private void SetServiceFilter(Guid? serviceId)
    {
        ServiceFilter = serviceId;
        RebuildFilteredImages();
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        if (!MediaPicker.Default.IsCaptureSupported) return;
        var photo = await MediaPicker.Default.CapturePhotoAsync();
        if (photo is not null)
            await UploadAsync(photo);
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        var photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 });
        var photo = photos?.FirstOrDefault();
        if (photo is not null)
            await UploadAsync(photo);
    }

    private async Task UploadAsync(FileResult photo)
    {
        if (OrderId is not Guid id) return;

        IsUploading = true;
        UploadError = null;

        try
        {
            await using var stream = await photo.OpenReadAsync();
            var uploaded = await _apiClient.UploadImageAsync(stream, photo.FileName, photo.ContentType ?? "image/jpeg");
            if (uploaded is null)
            {
                UploadError = "Upload failed.";
                return;
            }

            var response = await _apiClient.AddOrderImageAsync(
                id, new AddImageRequest(ImageTypeTab, uploaded.Url, ServiceFilter, null, string.IsNullOrWhiteSpace(UploadNotes) ? null : UploadNotes));

            if (response.IsSuccessStatusCode)
            {
                UploadNotes = null;
                await LoadImagesAsync();
            }
            else
            {
                UploadError = "Failed to attach the uploaded image.";
            }
        }
        catch (Exception ex)
        {
            UploadError = $"Couldn't upload the photo: {ex.Message}";
        }
        finally
        {
            IsUploading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteImageAsync(OrderGarmentImageDto? image)
    {
        if (image is null || OrderId is not Guid id) return;
        var confirmed = await Shell.Current.DisplayAlert("Remove image", "Remove this photo?", "Remove", "Cancel");
        if (!confirmed) return;

        await _apiClient.DeleteOrderImageAsync(id, image.Id);
        await LoadImagesAsync();
    }

    [RelayCommand]
    private async Task ConfirmPickupAsync()
    {
        if (PickupId is not Guid id) return;

        IsConfirming = true;
        ConfirmError = null;
        try
        {
            var response = await _apiClient.ConfirmPickupAsync(id);
            if (response.IsSuccessStatusCode)
                await Shell.Current.GoToAsync("//queue");
            else
                ConfirmError = "Upload at least one pickup photo before confirming.";
        }
        catch (Exception ex)
        {
            ConfirmError = $"Couldn't confirm pickup: {ex.Message}";
        }
        finally
        {
            IsConfirming = false;
        }
    }

    [RelayCommand]
    private async Task ConfirmDeliveryAsync()
    {
        if (DeliveryId is not Guid id) return;

        IsConfirming = true;
        ConfirmError = null;
        try
        {
            var response = await _apiClient.ConfirmDeliveryLegAsync(id);
            if (response.IsSuccessStatusCode)
                await Shell.Current.GoToAsync("//queue");
            else
                ConfirmError = "Upload at least one delivery photo before confirming.";
        }
        catch (Exception ex)
        {
            ConfirmError = $"Couldn't confirm delivery: {ex.Message}";
        }
        finally
        {
            IsConfirming = false;
        }
    }

    [RelayCommand]
    private async Task AssignAgentAsync(AssignableLeg? leg)
    {
        if (leg?.SelectedAgent is null) return;

        await _apiClient.AssignAgentAsync(leg.Leg.Id, leg.SelectedAgent.EmployeeId);
        await LoadPickupDeliveriesAsync();
    }

    partial void OnPickupIdChanged(Guid? value)
    {
        if (value.HasValue) ImageTypeTab = GarmentImageType.Pickup;
        OnPropertyChanged(nameof(CanUpload));
    }

    partial void OnDeliveryIdChanged(Guid? value)
    {
        if (value.HasValue) ImageTypeTab = GarmentImageType.Delivery;
        OnPropertyChanged(nameof(CanUpload));
    }

    partial void OnImageTypeTabChanged(GarmentImageType value) => OnPropertyChanged(nameof(CanUpload));
}
