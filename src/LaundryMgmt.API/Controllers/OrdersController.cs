using LaundryMgmt.Application.Common.Models;
using LaundryMgmt.Application.Orders.Commands.AdvanceOrderStatus;
using LaundryMgmt.Application.Orders.Commands.CreateOrder;
using LaundryMgmt.Application.Orders.Queries.GetOrders;
using LaundryMgmt.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryMgmt.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender) => _sender = sender;

    /// <summary>List orders with optional status filter and search, paginated.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<OrderListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<OrderListItemDto>>> GetOrders(
        [FromQuery] OrderStatus? status, [FromQuery] string? search,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(new GetOrdersQuery(status, search, pageNumber, pageSize));
        return Ok(result);
    }

    /// <summary>Create a new order (walk-in, pickup request, or express).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> CreateOrder(CreateOrderCommand command)
    {
        var orderId = await _sender.Send(command);
        return CreatedAtAction(nameof(GetOrders), new { id = orderId }, orderId);
    }

    /// <summary>Advance an order to the next step in the pipeline (or Cancelled).</summary>
    [HttpPatch("{orderId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AdvanceStatus(Guid orderId, [FromBody] OrderStatus newStatus)
    {
        await _sender.Send(new AdvanceOrderStatusCommand(orderId, newStatus));
        return NoContent();
    }
}
