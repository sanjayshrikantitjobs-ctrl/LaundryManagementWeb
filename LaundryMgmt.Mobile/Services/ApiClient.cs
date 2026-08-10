using System.Net.Http.Headers;
using System.Net.Http.Json;
using LaundryMgmt.Mobile.Models;
using LaundryMgmt.Shared.Auth;

namespace LaundryMgmt.Mobile.Services;

/// <summary>
/// Typed wrapper over HttpClient for everything the mobile app calls: auth,
/// the delivery-boy queue/OTP flow, and full Orders/Customers/Garments/Services/
/// Pricing CRUD for the admin/staff experience.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(string baseUrl)
    {
        var handler = new HttpClientHandler();
#if DEBUG
        // The API's local dev HTTPS certificate is self-signed; Android/iOS simulators
        // don't trust it the way the Windows dev machine does. DEBUG-only, compiled out
        // of Release builds.
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
        _http = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
    }

    public void SetBearerToken(string? token) =>
        _http.DefaultRequestHeaders.Authorization =
            token is null ? null : new AuthenticationHeaderValue("Bearer", token);

    public Task<LoginResponse?> LoginAsync(LoginRequest request) =>
        _http.PostAsJsonAsync("api/v1/auth/login", request)
            .ContinueWith(t => t.Result.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<LoginResponse>()).Unwrap();

    public Task<HttpResponseMessage> RegisterAsync(RegisterRequest request) =>
        _http.PostAsJsonAsync("api/v1/auth/register", request);

    public async Task<LoginResponse> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/v1/auth/verify-otp", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
    }

    // ---- Orders ----

    public Task<PaginatedList<OrderListItem>?> GetOrdersAsync(
        int pageNumber = 1, int pageSize = 20, OrderStatus? status = null, string? statuses = null) =>
        _http.GetFromJsonAsync<PaginatedList<OrderListItem>>(
            $"api/v1/orders?{Query(("pageNumber", pageNumber), ("pageSize", pageSize), ("status", status), ("statuses", statuses))}");

    public Task<PaginatedList<OrderListItem>?> GetMyOrdersAsync(
        int pageNumber = 1, int pageSize = 20, OrderStatus? status = null, string? statuses = null) =>
        _http.GetFromJsonAsync<PaginatedList<OrderListItem>>(
            $"api/v1/orders/mine?{Query(("pageNumber", pageNumber), ("pageSize", pageSize), ("status", status), ("statuses", statuses))}");

    public Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId) =>
        _http.GetFromJsonAsync<OrderDetailDto>($"api/v1/orders/{orderId}");

    public Task<HttpResponseMessage> CreateOrderAsync(CreateOrderRequest request) =>
        _http.PostAsJsonAsync("api/v1/orders", request);

    public Task<HttpResponseMessage> UpdateOrderAsync(Guid orderId, UpdateOrderRequest request) =>
        _http.PutAsJsonAsync($"api/v1/orders/{orderId}", request);

    // ---- Customers ----

    public Task<PaginatedList<CustomerListItem>?> GetCustomersAsync(
        string? search = null, int pageNumber = 1, int pageSize = 50, CustomerStatus? status = null) =>
        _http.GetFromJsonAsync<PaginatedList<CustomerListItem>>(
            $"api/v1/customers?{Query(("search", search), ("pageNumber", pageNumber), ("pageSize", pageSize), ("status", status))}");

    public Task<CustomerListItem?> GetMyCustomerProfileAsync() =>
        _http.GetFromJsonAsync<CustomerListItem>("api/v1/customers/me");

    public Task<HttpResponseMessage> UpdateMyProfileAsync(UpdateMyProfileRequest request) =>
        _http.PutAsJsonAsync("api/v1/customers/me", request);

    public Task<List<CustomerAddressDto>?> GetMyAddressesAsync() =>
        _http.GetFromJsonAsync<List<CustomerAddressDto>>("api/v1/customers/me/addresses");

    public Task<HttpResponseMessage> AddMyAddressAsync(CreateCustomerAddressRequest request) =>
        _http.PostAsJsonAsync("api/v1/customers/me/addresses", request);

    public Task<HttpResponseMessage> SetPrimaryAddressAsync(Guid addressId) =>
        _http.PutAsJsonAsync($"api/v1/customers/me/addresses/{addressId}/primary", new { });

    public Task<HttpResponseMessage> DeleteMyAddressAsync(Guid addressId) =>
        _http.DeleteAsync($"api/v1/customers/me/addresses/{addressId}");

    public Task<CustomerDetailDto?> GetCustomerByIdAsync(Guid id) =>
        _http.GetFromJsonAsync<CustomerDetailDto>($"api/v1/customers/{id}");

    public Task<HttpResponseMessage> CreateCustomerAsync(CreateCustomerRequest request) =>
        _http.PostAsJsonAsync("api/v1/customers", request);

    public Task<HttpResponseMessage> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request) =>
        _http.PutAsJsonAsync($"api/v1/customers/{id}", request);

    public Task<HttpResponseMessage> DeleteCustomerAsync(Guid id) =>
        _http.DeleteAsync($"api/v1/customers/{id}");

    public Task<HttpResponseMessage> SetCustomerStatusAsync(Guid id, CustomerStatus status) =>
        _http.PutAsJsonAsync($"api/v1/customers/{id}/status", new { status });

    // ---- Garments ----

    public Task<PaginatedList<GarmentListItem>?> GetGarmentsAsync(string? search = null, int pageNumber = 1, int pageSize = 200) =>
        _http.GetFromJsonAsync<PaginatedList<GarmentListItem>>(
            $"api/v1/garments?{Query(("search", search), ("pageNumber", pageNumber), ("pageSize", pageSize))}");

    public Task<GarmentDetailDto?> GetGarmentByIdAsync(Guid id) =>
        _http.GetFromJsonAsync<GarmentDetailDto>($"api/v1/garments/{id}");

    public Task<HttpResponseMessage> CreateGarmentAsync(CreateGarmentRequest request) =>
        _http.PostAsJsonAsync("api/v1/garments", request);

    public Task<HttpResponseMessage> UpdateGarmentAsync(Guid id, UpdateGarmentRequest request) =>
        _http.PutAsJsonAsync($"api/v1/garments/{id}", request);

    public Task<HttpResponseMessage> DeleteGarmentAsync(Guid id) =>
        _http.DeleteAsync($"api/v1/garments/{id}");

    public Task<HttpResponseMessage> SetGarmentPriceAsync(Guid garmentId, Guid serviceId, PricingType pricingType, decimal price) =>
        _http.PutAsJsonAsync($"api/v1/garments/{garmentId}/prices/{serviceId}", new { pricingType, price });

    public Task<PricingMatrixDto?> GetPricingMatrixAsync() =>
        _http.GetFromJsonAsync<PricingMatrixDto>("api/v1/garments/pricing-matrix");

    // ---- Services ----

    public Task<PaginatedList<ServiceListItem>?> GetServicesAsync(string? search = null, int pageNumber = 1, int pageSize = 200) =>
        _http.GetFromJsonAsync<PaginatedList<ServiceListItem>>(
            $"api/v1/services?{Query(("search", search), ("pageNumber", pageNumber), ("pageSize", pageSize))}");

    public Task<HttpResponseMessage> CreateServiceAsync(CreateServiceRequest request) =>
        _http.PostAsJsonAsync("api/v1/services", request);

    public Task<HttpResponseMessage> DeleteServiceAsync(Guid id) =>
        _http.DeleteAsync($"api/v1/services/{id}");

    // ---- Users (Admin-only) ----

    public Task<PaginatedList<UserSummaryDto>?> GetUsersAsync(
        string? search = null, UserRole? role = null, bool? isActive = null, int pageNumber = 1, int pageSize = 20) =>
        _http.GetFromJsonAsync<PaginatedList<UserSummaryDto>>(
            $"api/v1/users?{Query(("search", search), ("role", role), ("isActive", isActive), ("pageNumber", pageNumber), ("pageSize", pageSize))}");

    public Task<UserSummaryDto?> GetUserByIdAsync(Guid id) =>
        _http.GetFromJsonAsync<UserSummaryDto>($"api/v1/users/{id}");

    public Task<HttpResponseMessage> CreateUserAsync(CreateUserRequest request) =>
        _http.PostAsJsonAsync("api/v1/users", request);

    public Task<HttpResponseMessage> UpdateUserAsync(Guid id, UpdateUserRequest request) =>
        _http.PutAsJsonAsync($"api/v1/users/{id}", request);

    public Task<HttpResponseMessage> SetUserActiveAsync(Guid id, bool isActive) =>
        _http.PutAsJsonAsync($"api/v1/users/{id}/active", new { isActive });

    public Task<HttpResponseMessage> AssignRoleAsync(Guid id, UserRole role) =>
        _http.PutAsJsonAsync($"api/v1/users/{id}/role", new { role });

    // ---- Uploads ----

    public async Task<UploadedImageDto?> UploadImageAsync(Stream fileStream, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        var response = await _http.PostAsync("api/v1/uploads/images", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UploadedImageDto>();
    }

    /// <summary>Downloads image bytes through this client's HttpClient rather than
    /// letting an Image control load the URL directly — MAUI's built-in Image/UriImageSource
    /// networking doesn't share the DEBUG-only self-signed-cert bypass configured above, so
    /// direct URL binding silently fails to load images on Android/iOS even though every
    /// other API call (which does go through this HttpClient) works fine. See
    /// Behaviors/TrustedImage.cs for the attached property that calls this.</summary>
    public async Task<byte[]?> DownloadImageBytesAsync(string url)
    {
        try
        {
            return await _http.GetByteArrayAsync(url);
        }
        catch
        {
            return null;
        }
    }

    // ---- Order Garment Images ----

    public Task<List<OrderGarmentImageDto>?> GetOrderImagesAsync(Guid orderId) =>
        _http.GetFromJsonAsync<List<OrderGarmentImageDto>>($"api/v1/orders/{orderId}/images");

    public Task<HttpResponseMessage> AddOrderImageAsync(Guid orderId, AddImageRequest request) =>
        _http.PostAsJsonAsync($"api/v1/orders/{orderId}/images", request);

    public Task<HttpResponseMessage> DeleteOrderImageAsync(Guid orderId, Guid imageId) =>
        _http.DeleteAsync($"api/v1/orders/{orderId}/images/{imageId}");

    // ---- Pickup/Delivery ----

    public Task<PaginatedList<MyPickupDeliveryDto>?> GetMyPickupDeliveriesAsync(int pageNumber = 1, int pageSize = 20) =>
        _http.GetFromJsonAsync<PaginatedList<MyPickupDeliveryDto>>(
            $"api/v1/pickup-delivery/mine?{Query(("pageNumber", pageNumber), ("pageSize", pageSize))}");

    public Task<HttpResponseMessage> ConfirmPickupAsync(Guid id) =>
        _http.PostAsync($"api/v1/pickup-delivery/{id}/confirm-pickup", null);

    public Task<HttpResponseMessage> ConfirmDeliveryLegAsync(Guid id) =>
        _http.PostAsync($"api/v1/pickup-delivery/{id}/confirm-delivery", null);

    public Task<List<OrderPickupDeliveryDto>?> GetOrderPickupDeliveriesAsync(Guid orderId) =>
        _http.GetFromJsonAsync<List<OrderPickupDeliveryDto>>($"api/v1/pickup-delivery/order/{orderId}");

    public Task<List<AgentDto>?> GetAgentsAsync(UserRole role) =>
        _http.GetFromJsonAsync<List<AgentDto>>($"api/v1/pickup-delivery/agents?{Query(("role", role))}");

    public Task<HttpResponseMessage> AssignAgentAsync(Guid id, Guid employeeId) =>
        _http.PutAsJsonAsync($"api/v1/pickup-delivery/{id}/assign", new AssignAgentRequest(employeeId));

    // ---- Promotions ----

    public Task<List<ActivePromotionDto>?> GetActivePromotionsAsync() =>
        _http.GetFromJsonAsync<List<ActivePromotionDto>>("api/v1/promotions/active");

    // ---- Contact Messages ----

    public Task<List<MyContactMessageDto>?> GetMyContactMessagesAsync() =>
        _http.GetFromJsonAsync<List<MyContactMessageDto>>("api/v1/contactmessages/mine");

    public Task<HttpResponseMessage> CreateContactMessageAsync(CreateContactMessageRequest request) =>
        _http.PostAsJsonAsync("api/v1/contactmessages", request);

    private static string Query(params (string Key, object? Value)[] parameters)
    {
        var pairs = parameters
            .Where(p => p.Value is not null && (p.Value is not string s || !string.IsNullOrWhiteSpace(s)))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!.ToString()!)}");
        return string.Join("&", pairs);
    }
}
