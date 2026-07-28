using FluentValidation;
using LaundryMgmt.Application.Common.Interfaces;
using LaundryMgmt.Domain.Entities;
using LaundryMgmt.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaundryMgmt.Application.Orders.Commands.CreateOrder;

public record CreateOrderItemDto(Guid GarmentId, Guid ServiceId, int Quantity, decimal? WeightKg, string? SpecialInstructions);

public record CreateOrderCommand(
    Guid CustomerId,
    OrderChannel Channel,
    bool IsExpress,
    List<CreateOrderItemDto> Items) : IRequest<Guid>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("An order must contain at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.GarmentId).NotEmpty();
            item.RuleFor(i => i.ServiceId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateOrderCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = request.CustomerId,
            Channel = request.Channel,
            IsExpress = request.IsExpress
        };

        foreach (var itemDto in request.Items)
        {
            var priceEntry = await _db.GarmentServicePrices
                .FirstOrDefaultAsync(p => p.GarmentId == itemDto.GarmentId && p.ServiceId == itemDto.ServiceId, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"No price configured for garment {itemDto.GarmentId} + service {itemDto.ServiceId}.");

            var unitPrice = priceEntry.PricingType == PricingType.WeightBased && itemDto.WeightKg.HasValue
                ? priceEntry.Price * itemDto.WeightKg.Value
                : priceEntry.Price;

            var lineTotal = priceEntry.PricingType == PricingType.WeightBased
                ? unitPrice
                : unitPrice * itemDto.Quantity;

            order.Items.Add(new OrderItem
            {
                GarmentId = itemDto.GarmentId,
                ServiceId = itemDto.ServiceId,
                Quantity = itemDto.Quantity,
                WeightKg = itemDto.WeightKg,
                UnitPrice = unitPrice,
                LineTotal = lineTotal,
                SpecialInstructions = itemDto.SpecialInstructions
            });
        }

        order.SubTotal = order.Items.Sum(i => i.LineTotal);
        // TODO: apply discounts/coupons/membership pricing here (Pricing module)
        order.GstAmount = Math.Round(order.SubTotal * 0.05m, 2); // placeholder flat 5% GST, wire to Service.GstPercentage
        order.TotalAmount = order.SubTotal + order.GstAmount - order.DiscountAmount + order.DeliveryCharge;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(100000, 999999)}";
}
