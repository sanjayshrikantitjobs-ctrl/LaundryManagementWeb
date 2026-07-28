using LaundryMgmt.Domain.Enums;

namespace LaundryMgmt.Application.Common.Interfaces;

public record AuthenticatedUserDto(Guid UserId, string UserName, string Email, string FullName, UserRole Role);

/// <summary>
/// Thin abstraction over ASP.NET Core Identity so Application-layer handlers
/// never take a dependency on Microsoft.AspNetCore.Identity directly.
/// Implemented in Infrastructure/Identity/IdentityAuthService.cs.
/// </summary>
public interface IIdentityAuthService
{
    Task<AuthenticatedUserDto?> ValidateCredentialsAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default);
}

public interface ITokenService
{
    (string accessToken, DateTimeOffset expiresAtUtc) GenerateAccessToken(AuthenticatedUserDto user);
    string GenerateRefreshToken();
}
