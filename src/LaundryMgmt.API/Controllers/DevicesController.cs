using LaundryMgmt.Application.Devices.Commands.RegisterDeviceToken;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryMgmt.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly ISender _sender;

    public DevicesController(ISender sender) => _sender = sender;

    /// <summary>Registers (or refreshes) the caller's FCM device token, so push
    /// notifications can reach them.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Register(RegisterDeviceTokenCommand command)
    {
        await _sender.Send(command);
        return NoContent();
    }
}
