using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using LaundryMgmt.Application.Common.Interfaces;
using LaundryMgmt.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LaundryMgmt.Infrastructure.Services;

/// <summary>OS-level push via Firebase Cloud Messaging (Android only for now). Distinct
/// from <see cref="NotificationService"/> (SMS/WhatsApp, unimplemented) and the in-app
/// Notification feed — this is the one that reaches a customer while the app is closed.
/// Per-token fan-out (not FCM topics): at this app's scale (a single local business,
/// realistically dozens to low hundreds of devices) topics buy nothing — their up-to-24h
/// subscription propagation and lack of per-message delivery results are solving a
/// scale problem this app doesn't have, whereas SendEachAsync's batched per-token result
/// lets a future cleanup job prune tokens Firebase reports as unregistered.</summary>
public class PushNotificationService : IPushNotificationService
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<PushNotificationService> _logger;
    private static readonly object InitLock = new();
    private static bool _initAttempted;
    private static bool _available;

    public PushNotificationService(IApplicationDbContext db, IConfiguration configuration, ILogger<PushNotificationService> logger)
    {
        _db = db;
        _logger = logger;
        EnsureInitialized(configuration, logger);
    }

    // Must never throw: this constructor runs whenever MediatR resolves a handler that
    // depends on IPushNotificationService (OrderStatusChangedPushHandler,
    // PromotionCreatedPushHandler), which happens *inside* ApplicationDbContext.
    // SaveChangesAsync's event-publishing loop — after the real change (order status,
    // new promotion) has already been committed by base.SaveChangesAsync(). Throwing
    // here would surface as the whole request failing with a misleading error, even
    // though the actual data change already succeeded. A missing/bad credential should
    // only ever mean "no push gets sent", never "the request fails".
    private static void EnsureInitialized(IConfiguration configuration, ILogger logger)
    {
        if (_initAttempted) return;
        lock (InitLock)
        {
            if (_initAttempted) return;
            _initAttempted = true;

            var json = configuration["Firebase:ServiceAccountJson"];
            if (string.IsNullOrWhiteSpace(json))
            {
                logger.LogWarning(
                    "Firebase:ServiceAccountJson is not configured — push notifications are disabled. " +
                    "Set it via `dotnet user-secrets` locally or the Firebase__ServiceAccountJson app setting in Azure.");
                return;
            }

            try
            {
                if (FirebaseApp.DefaultInstance is null)
                    FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromJson(json) });

                _available = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize Firebase — push notifications are disabled.");
            }
        }
    }

    public async Task SendToUserAsync(Guid userId, string title, string body, string? entityId, NotificationType type, CancellationToken cancellationToken = default)
    {
        if (!_available) return;

        var tokens = await _db.DeviceTokens
            .Where(d => d.UserId == userId)
            .Select(d => d.Token)
            .ToListAsync(cancellationToken);

        if (tokens.Count == 0) return;

        await SendToTokensAsync(tokens, title, body, entityId, type, cancellationToken);
    }

    public async Task SendToAllCustomersAsync(string title, string body, string? entityId, NotificationType type, CancellationToken cancellationToken = default)
    {
        if (!_available) return;

        var tokens = await _db.DeviceTokens
            .Select(d => d.Token)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (tokens.Count == 0) return;

        await SendToTokensAsync(tokens, title, body, entityId, type, cancellationToken);
    }

    private async Task SendToTokensAsync(List<string> tokens, string title, string body, string? entityId, NotificationType type, CancellationToken cancellationToken)
    {
        var data = new Dictionary<string, string> { ["type"] = type.ToString() };
        if (entityId is not null) data["entityId"] = entityId;

        // SendEachAsync batches internally (FCM's server-side limit is 500/call) and
        // returns a per-token result instead of failing the whole batch on one bad token.
        var messages = tokens.Select(token => new Message
        {
            Token = token,
            Notification = new FirebaseAdmin.Messaging.Notification { Title = title, Body = body },
            Data = data
        }).ToList();

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendEachAsync(messages, cancellationToken);
            if (response.FailureCount > 0)
                _logger.LogWarning("Push send: {Success} succeeded, {Failure} failed out of {Total}.",
                    response.SuccessCount, response.FailureCount, messages.Count);
        }
        catch (Exception ex)
        {
            // Best-effort — a push failure should never fail the command that triggered it
            // (order status change / promotion creation already committed to the DB).
            _logger.LogError(ex, "Failed to send push notification.");
        }
    }
}
