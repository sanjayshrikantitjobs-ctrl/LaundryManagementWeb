using LaundryMgmt.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace LaundryMgmt.Infrastructure.Identity;

public class IdentityAuthService : IIdentityAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityAuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<AuthenticatedUserDto?> ValidateCredentialsAsync(
        string usernameOrEmail, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(usernameOrEmail)
                   ?? await _userManager.FindByEmailAsync(usernameOrEmail);

        if (user is null || !user.IsActive)
            return null;

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
            return null;

        return new AuthenticatedUserDto(user.Id, user.UserName ?? user.Email ?? string.Empty,
            user.Email ?? string.Empty, user.FullName, user.Role);
    }
}

/// <summary>Adapts the concrete JwtTokenService (which speaks ApplicationUser)
/// to the Application-layer's ITokenService (which speaks AuthenticatedUserDto),
/// so Application never references Microsoft.AspNetCore.Identity.</summary>
public class TokenServiceAdapter : ITokenService
{
    private readonly IJwtTokenService _jwtTokenService;

    public TokenServiceAdapter(IJwtTokenService jwtTokenService) => _jwtTokenService = jwtTokenService;

    public (string accessToken, DateTimeOffset expiresAtUtc) GenerateAccessToken(AuthenticatedUserDto user)
    {
        var pseudoUser = new ApplicationUser
        {
            Id = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role
        };
        return _jwtTokenService.GenerateAccessToken(pseudoUser);
    }

    public string GenerateRefreshToken() => _jwtTokenService.GenerateRefreshToken();
}
