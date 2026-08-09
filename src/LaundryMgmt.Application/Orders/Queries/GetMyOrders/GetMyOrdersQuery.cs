using LaundryMgmt.Application.Common.Interfaces;
using LaundryMgmt.Application.Common.Models;
using LaundryMgmt.Application.Orders.Queries.GetOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaundryMgmt.Application.Orders.Queries.GetMyOrders;

/// <summary>Order history for the logged-in customer only — resolves the caller's
/// linked Customer catalog row via Customer.IdentityUserId, then scopes to it.
/// Reuses OrderListItemDto so the mobile/web clients share one shape with the
/// admin-wide GetOrdersQuery.</summary>
public record GetMyOrdersQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<OrderListItemDto>>;

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, PaginatedList<OrderListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyOrdersQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<OrderListItemDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Not signed in.");

        var customerId = await _db.Customers
            .Where(c => c.IdentityUserId == userId)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No customer profile is linked to this login.");

        var query = _db.Orders.Include(o => o.Customer).Where(o => o.CustomerId == customerId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderListItemDto(
                o.Id, o.OrderNumber, o.Customer!.FullName, o.Status,
                o.TotalAmount, o.PaymentStatus, o.CreatedAtUtc, o.PromisedByUtc))
            .ToListAsync(cancellationToken);

        return new PaginatedList<OrderListItemDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
