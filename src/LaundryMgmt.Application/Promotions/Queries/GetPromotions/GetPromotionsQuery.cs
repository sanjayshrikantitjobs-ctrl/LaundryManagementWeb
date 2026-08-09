using LaundryMgmt.Application.Common.Interfaces;
using LaundryMgmt.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaundryMgmt.Application.Promotions.Queries.GetPromotions;

public record PromotionListItemDto(
    Guid Id, string Title, string? Description, string? ImageUrl, string? Code,
    decimal? DiscountPercent, decimal? DiscountAmount,
    DateTimeOffset? ValidFrom, DateTimeOffset? ValidTo, bool IsActive);

/// <summary>Admin-facing listing — returns every promotion regardless of active/validity status.</summary>
public record GetPromotionsQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<PromotionListItemDto>>;

public class GetPromotionsQueryHandler : IRequestHandler<GetPromotionsQuery, PaginatedList<PromotionListItemDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPromotionsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<PromotionListItemDto>> Handle(GetPromotionsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Promotions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Title.Contains(request.Search));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PromotionListItemDto(
                p.Id, p.Title, p.Description, p.ImageUrl, p.Code,
                p.DiscountPercent, p.DiscountAmount, p.ValidFrom, p.ValidTo, p.IsActive))
            .ToListAsync(cancellationToken);

        return new PaginatedList<PromotionListItemDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
