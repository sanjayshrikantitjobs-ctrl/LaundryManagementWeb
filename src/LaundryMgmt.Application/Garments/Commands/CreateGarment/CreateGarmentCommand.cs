using FluentValidation;
using LaundryMgmt.Application.Common.Interfaces;
using LaundryMgmt.Domain.Entities;
using MediatR;

namespace LaundryMgmt.Application.Garments.Commands.CreateGarment;

public record CreateGarmentCommand(
    string Name, string Category, string? Barcode, string? SpecialInstructions) : IRequest<Guid>;

public class CreateGarmentCommandValidator : AbstractValidator<CreateGarmentCommand>
{
    public CreateGarmentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Barcode).MaximumLength(64);
    }
}

public class CreateGarmentCommandHandler : IRequestHandler<CreateGarmentCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateGarmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Guid> Handle(CreateGarmentCommand request, CancellationToken cancellationToken)
    {
        var garment = new Garment
        {
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            Barcode = request.Barcode,
            SpecialInstructions = request.SpecialInstructions
        };

        _db.Garments.Add(garment);
        await _db.SaveChangesAsync(cancellationToken);

        return garment.Id;
    }
}
