using FluentValidation;
using LaundryMgmt.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaundryMgmt.Application.Garments.Commands.UpdateGarment;

public record UpdateGarmentCommand(
    Guid GarmentId, string Name, string Category, string? Barcode, string? SpecialInstructions, string? ImageUrl = null) : IRequest;

public class UpdateGarmentCommandValidator : AbstractValidator<UpdateGarmentCommand>
{
    public UpdateGarmentCommandValidator()
    {
        RuleFor(x => x.GarmentId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Barcode).MaximumLength(64);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}

public class UpdateGarmentCommandHandler : IRequestHandler<UpdateGarmentCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateGarmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateGarmentCommand request, CancellationToken cancellationToken)
    {
        var garment = await _db.Garments
            .FirstOrDefaultAsync(g => g.Id == request.GarmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Garment {request.GarmentId} not found.");

        garment.Name = request.Name.Trim();
        garment.Category = request.Category.Trim();
        garment.Barcode = request.Barcode;
        garment.SpecialInstructions = request.SpecialInstructions;
        garment.ImageUrl = request.ImageUrl;

        await _db.SaveChangesAsync(cancellationToken);
    }
}
