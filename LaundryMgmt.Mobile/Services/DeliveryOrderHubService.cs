using Microsoft.AspNetCore.SignalR.Client;

namespace LaundryMgmt.Mobile.Services;

/// <summary>Mirrors the payload shape sent by OrderStatusChangedSignalRHandler
/// in the API project — keep these two in sync.</summary>
public record OrderStatusUpdate(Guid OrderId, string OrderNumber, int NewStatus);

public class DeliveryOrderHubService : IAsyncDisposable
{
    private HubConnection? _connection;

    public event Action<OrderStatusUpdate>? OrderStatusChanged;

    public async Task ConnectAsync(string baseUrl, string accessToken)
    {
        if (_connection is not null) return;

        _connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/order-status", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<OrderStatusUpdate>("OrderStatusChanged", update => OrderStatusChanged?.Invoke(update));

        await _connection.StartAsync();
    }

    public async Task JoinOrderGroupAsync(Guid orderId) =>
        await (_connection?.InvokeAsync("JoinOrderGroup", orderId.ToString()) ?? Task.CompletedTask);

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
