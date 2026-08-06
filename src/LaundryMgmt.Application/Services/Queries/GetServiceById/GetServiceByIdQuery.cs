using LaundryMgmt.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaundryMgmt.Application.Services.Queries.GetServiceById;

public record ServiceDetailDto(
    Guid Id, string Name, decimal BasePrice, int EstimatedTimeHours, decimal GstPercentage, int Priority);

public record GetServiceByIdQuery(Guid ServiceId) : IRequest<ServiceDetailDto>;

public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ServiceDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetServiceByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ServiceDetailDto> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _db.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Service {request.ServiceId} not found.");

        return new ServiceDetailDto(
            service.Id, service.Name, service.BasePrice, service.EstimatedTimeHours, service.GstPercentage, service.Priority);
    }
}
