using FluentValidation;
using LaundryMgmt.Application.Common.Interfaces;
using MediatR;

namespace LaundryMgmt.Application.Auth.Commands.Login;

public record LoginCommand(string UsernameOrEmail, string Password) : IRequest<LoginResultDto>;

public record LoginResultDto(
    string AccessToken, string RefreshToken, DateTimeOffset ExpiresAtUtc,
    Guid UserId, string FullName, string Role);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private readonly IIdentityAuthService _identityAuth;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IIdentityAuthService identityAuth, ITokenService tokenService)
    {
        _identityAuth = identityAuth;
        _tokenService = tokenService;
    }

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityAuth.ValidateCredentialsAsync(request.UsernameOrEmail, request.Password, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid username or password.");

        var (accessToken, expiresAtUtc) = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // NOTE: persisting/rotating the refresh token against the user record
        // happens in IIdentityAuthService's implementation or a follow-up
        // SaveRefreshTokenAsync call — kept out of this handler to keep the
        // Application layer free of storage concerns.

        return new LoginResultDto(accessToken, refreshToken, expiresAtUtc, user.UserId, user.FullName, user.Role.ToString());
    }
}
