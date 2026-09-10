using FluentValidation;
using LaundryMgmt.Application.Common.Interfaces;
using LaundryMgmt.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaundryMgmt.Application.Devices.Commands.RegisterDeviceToken;

/// <summary>Registers (or refreshes) this device's FCM token against the caller's
/// login, so push notifications can be sent to them later. Called by the mobile app
/// right after login, and again on startup for an already-logged-in session (FCM
/// tokens can rotate).</summary>
public record RegisterDeviceTokenCommand(string Token, string Platform) : IRequest;

public class RegisterDeviceTokenCommandValidator : AbstractValidator<RegisterDeviceTokenCommand>
{
    public RegisterDeviceTokenCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Platform).NotEmpty().MaximumLength(20);
    }
}

public class RegisterDeviceTokenCommandHandler : IRequestHandler<RegisterDeviceTokenCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RegisterDeviceTokenCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(RegisterDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("No authenticated user.");

        // Upsert by token, not by (userId, platform) — the same physical device can be
        // logged into a different account later (e.g. shared device), and the token is
        // what FCM actually addresses, so it must stay unique.
        var existing = await _db.DeviceTokens.FirstOrDefaultAsync(d => d.Token == request.Token, cancellationToken);

        if (existing is not null)
        {
            existing.UserId = userId;
            existing.Platform = request.Platform;
            existing.LastSeenUtc = DateTimeOffset.UtcNow;
        }
        else
        {
            _db.DeviceTokens.Add(new DeviceToken
            {
                UserId = userId,
                Token = request.Token,
                Platform = request.Platform,
                LastSeenUtc = DateTimeOffset.UtcNow
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
