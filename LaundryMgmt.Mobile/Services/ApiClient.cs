using System.Net.Http.Headers;
using System.Net.Http.Json;
using LaundryMgmt.Shared.Auth;

namespace LaundryMgmt.Mobile.Services;

/// <summary>
/// Minimal typed wrapper over HttpClient for the endpoints the staff/delivery
/// app needs: login, my assigned pickups/deliveries, and OTP confirmation.
/// Extend with barcode-scan lookups as the Barcode/QR module comes online.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public void SetBearerToken(string token) =>
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public Task<LoginResponse?> LoginAsync(LoginRequest request) =>
        _http.PostAsJsonAsync("api/v1/auth/login", request)
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<LoginResponse>()).Unwrap();

    public Task<HttpResponseMessage> ConfirmDeliveryAsync(DeliveryOtpConfirmationRequest request) =>
        _http.PostAsJsonAsync("api/v1/pickup-delivery/confirm", request);

    // TODO: GetMyAssignedOrdersAsync() once the Pickup/Delivery API controller exists.
}
