using LaundryMgmt.Application.Auth.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryMgmt.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender) => _sender = sender;

    /// <summary>Authenticates a staff/admin/delivery-boy user and issues a JWT
    /// access token + refresh token. Used by both the Angular app and the MAUI app.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResultDto>> Login(LoginCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }
}
