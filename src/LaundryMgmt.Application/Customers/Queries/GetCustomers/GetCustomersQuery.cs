using LaundryMgmt.Application.Common.Interfaces;
using LaundryMgmt.Application.Common.Models;
using LaundryMgmt.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaundryMgmt.Application.Customers.Queries.GetCustomers;

public record CustomerListItemDto(
    Guid Id, string FullName, string PhoneNumber, string? Email,
    MembershipTier MembershipTier, decimal WalletBalance, int LoyaltyPoints);

public record GetCustomersQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<CustomerListItemDto>>;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PaginatedList<CustomerListItemDto>>
{
    private readonly IApplicationDbContext _db;

    public GetCustomersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<CustomerListItemDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c =>
                c.FullName.Contains(request.Search) ||
                c.PhoneNumber.Contains(request.Search) ||
                (c.Email != null && c.Email.Contains(request.Search)));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CustomerListItemDto(
                c.Id, c.FullName, c.PhoneNumber, c.Email, c.MembershipTier, c.WalletBalance, c.LoyaltyPoints))
            .ToListAsync(cancellationToken);

        return new PaginatedList<CustomerListItemDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
