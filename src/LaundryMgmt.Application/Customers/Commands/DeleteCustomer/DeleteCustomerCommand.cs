using LaundryMgmt.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaundryMgmt.Application.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(Guid CustomerId) : IRequest;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteCustomerCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {request.CustomerId} not found.");

        // AuditableEntitySaveChangesInterceptor turns this into a soft delete.
        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
