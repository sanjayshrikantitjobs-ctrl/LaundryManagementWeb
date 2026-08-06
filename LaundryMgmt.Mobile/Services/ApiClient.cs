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

    public Task<HttpResponseMessage> ConfirmDeliveryAsync(DeliveryOtpConfirmationRequest request) =>
        _http.PostAsJsonAsync("api/v1/pickup-delivery/confirm", request);

    // ---- Orders ----

    public Task<PaginatedList<OrderListItem>?> GetOrdersAsync(int pageNumber = 1, int pageSize = 20) =>
        _http.GetFromJsonAsync<PaginatedList<OrderListItem>>(
            $"api/v1/orders?{Query(("pageNumber", pageNumber), ("pageSize", pageSize))}");

    public Task<HttpResponseMessage> CreateOrderAsync(CreateOrderRequest request) =>
        _http.PostAsJsonAsync("api/v1/orders", request);

    // ---- Customers ----

    public Task<PaginatedList<CustomerListItem>?> GetCustomersAsync(string? search = null, int pageNumber = 1, int pageSize = 50) =>
        _http.GetFromJsonAsync<PaginatedList<CustomerListItem>>(
            $"api/v1/customers?{Query(("search", search), ("pageNumber", pageNumber), ("pageSize", pageSize))}");

    public Task<HttpResponseMessage> CreateCustomerAsync(CreateCustomerRequest request) =>
        _http.PostAsJsonAsync("api/v1/customers", request);

    public Task<HttpResponseMessage> DeleteCustomerAsync(Guid id) =>
        _http.DeleteAsync($"api/v1/customers/{id}");

    // ---- Garments ----

    public Task<PaginatedList<GarmentListItem>?> GetGarmentsAsync(string? search = null, int pageNumber = 1, int pageSize = 200) =>
        _http.GetFromJsonAsync<PaginatedList<GarmentListItem>>(
            $"api/v1/garments?{Query(("search", search), ("pageNumber", pageNumber), ("pageSize", pageSize))}");

    public Task<GarmentDetailDto?> GetGarmentByIdAsync(Guid id) =>
        _http.GetFromJsonAsync<GarmentDetailDto>($"api/v1/garments/{id}");

    public Task<HttpResponseMessage> CreateGarmentAsync(CreateGarmentRequest request) =>
        _http.PostAsJsonAsync("api/v1/garments", request);

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

    private static string Query(params (string Key, object? Value)[] parameters)
    {
        var pairs = parameters
            .Where(p => p.Value is not null && (p.Value is not string s || !string.IsNullOrWhiteSpace(s)))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!.ToString()!)}");
        return string.Join("&", pairs);
    }
}
