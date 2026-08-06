using LaundryMgmt.Application.Common.Models;
using LaundryMgmt.Application.Customers.Commands.AddCustomerAddress;
using LaundryMgmt.Application.Customers.Commands.CreateCustomer;
using LaundryMgmt.Application.Customers.Commands.DeleteCustomer;
using LaundryMgmt.Application.Customers.Commands.UpdateCustomer;
using LaundryMgmt.Application.Customers.Queries.GetCustomerById;
using LaundryMgmt.Application.Customers.Queries.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryMgmt.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender) => _sender = sender;

    /// <summary>List customers with optional search, paginated.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<CustomerListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<CustomerListItemDto>>> GetCustomers(
        [FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(new GetCustomersQuery(search, pageNumber, pageSize));
        return Ok(result);
    }

    /// <summary>Get a single customer with their addresses.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerDetailDto>> GetCustomerById(Guid id)
    {
        var result = await _sender.Send(new GetCustomerByIdQuery(id));
        return Ok(result);
    }

    /// <summary>Register a new customer, optionally with initial addresses.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> CreateCustomer(CreateCustomerCommand command)
    {
        var customerId = await _sender.Send(command);
        return CreatedAtAction(nameof(GetCustomerById), new { id = customerId }, customerId);
    }

    /// <summary>Update a customer's profile.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerBody body)
    {
        await _sender.Send(new UpdateCustomerCommand(
            id, body.FullName, body.PhoneNumber, body.Email, body.CreditLimit, body.MembershipTier, body.Notes));
        return NoContent();
    }

    /// <summary>Soft-delete a customer.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        await _sender.Send(new DeleteCustomerCommand(id));
        return NoContent();
    }

    /// <summary>Add an address to an existing customer.</summary>
    [HttpPost("{id:guid}/addresses")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> AddAddress(Guid id, [FromBody] AddAddressBody body)
    {
        var addressId = await _sender.Send(new AddCustomerAddressCommand(
            id, body.Label, body.Line1, body.Line2, body.City, body.State, body.PostalCode, body.IsDefault));
        return CreatedAtAction(nameof(GetCustomerById), new { id }, addressId);
    }
}

public record UpdateCustomerBody(
    string FullName, string PhoneNumber, string? Email, decimal CreditLimit,
    LaundryMgmt.Domain.Enums.MembershipTier MembershipTier, string? Notes);

public record AddAddressBody(
    string Label, string Line1, string? Line2, string City, string State, string PostalCode, bool IsDefault);
