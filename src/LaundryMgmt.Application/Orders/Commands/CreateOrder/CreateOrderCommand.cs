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
    List<CreateOrderItemDto> Items,
    DateTimeOffset? PreferredPickupAtUtc = null,
    Guid? PickupAddressId = null,
    string? PromoCode = null) : IRequest<Guid>;

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
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public CreateOrderCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // A Customer-role caller may only ever place an order under their own linked
        // Customer record — otherwise they could pass any CustomerId in the request body
        // and place orders (and see the resulting invoice) under someone else's identity.
        if (string.Equals(_currentUser.Role, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            var ownCustomerId = await _db.Customers
                .Where(c => c.IdentityUserId == _currentUser.UserId)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (ownCustomerId != request.CustomerId)
                throw new UnauthorizedAccessException("You can only place orders for your own account.");
        }

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = request.CustomerId,
            Channel = request.Channel,
            IsExpress = request.IsExpress
        };

        var maxEtaHours = 0;

        foreach (var itemDto in request.Items)
        {
            var priceEntry = await _db.GarmentServicePrices
                .Include(p => p.Service)
                .FirstOrDefaultAsync(p => p.GarmentId == itemDto.GarmentId && p.ServiceId == itemDto.ServiceId, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"No price configured for garment {itemDto.GarmentId} + service {itemDto.ServiceId}.");

            var service = priceEntry.Service!;

            var unitPrice = priceEntry.PricingType == PricingType.WeightBased && itemDto.WeightKg.HasValue
                ? priceEntry.Price * itemDto.WeightKg.Value
                : priceEntry.Price;

            // Express channel adds the service's configured per-item surcharge and
            // shortens the promised turnaround to its express ETA (both admin-set on
            // the Service — e.g. Steam Iron: +₹10 / 1 hour; Dry Clean/Wash: +₹40 / 24 hours).
            if (request.Channel == OrderChannel.Express)
                unitPrice += service.ExpressSurcharge;

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

            var etaHours = request.Channel == OrderChannel.Express ? service.ExpressEtaHours : service.EstimatedTimeHours;
            maxEtaHours = Math.Max(maxEtaHours, etaHours);
        }

        order.SubTotal = order.Items.Sum(i => i.LineTotal);

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var now = _dateTime.UtcNow;
            var normalizedCode = request.PromoCode.Trim().ToUpperInvariant();

            var promotion = await _db.Promotions.FirstOrDefaultAsync(p =>
                p.Code != null && p.Code == normalizedCode &&
                p.IsActive &&
                (!p.ValidFrom.HasValue || p.ValidFrom <= now) &&
                (!p.ValidTo.HasValue || p.ValidTo >= now),
                cancellationToken)
                ?? throw new InvalidOperationException($"Promo code '{request.PromoCode}' is invalid or has expired.");

            var discount = promotion.DiscountPercent.HasValue
                ? order.SubTotal * promotion.DiscountPercent.Value / 100m
                : promotion.DiscountAmount ?? 0m;

            order.DiscountAmount = Math.Round(Math.Min(discount, order.SubTotal), 2);
            order.PromoCode = normalizedCode;
        }

        order.GstAmount = Math.Round(order.SubTotal * 0.05m, 2); // placeholder flat 5% GST, wire to Service.GstPercentage
        order.TotalAmount = order.SubTotal + order.GstAmount - order.DiscountAmount + order.DeliveryCharge;
        order.PromisedByUtc = DateTimeOffset.UtcNow.AddHours(maxEtaHours);

        _db.Orders.Add(order);

        var customerName = await _db.Customers
            .Where(c => c.Id == request.CustomerId)
            .Select(c => c.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        _db.Notifications.Add(new Notification
        {
            RecipientRole = "Admin",
            Type = NotificationType.NewOrder,
            Title = "New order placed",
            Message = $"{customerName ?? "A customer"} placed order {order.OrderNumber} (₹{order.TotalAmount}).",
            EntityId = order.Id.ToString()
        });

        if (request.PreferredPickupAtUtc.HasValue)
        {
            Guid? addressId = null;
            if (request.PickupAddressId.HasValue)
            {
                var addressBelongsToCustomer = await _db.CustomerAddresses.AnyAsync(
                    a => a.Id == request.PickupAddressId.Value && a.CustomerId == request.CustomerId, cancellationToken);
                addressId = addressBelongsToCustomer ? request.PickupAddressId : null;
            }

            _db.PickupDeliveries.Add(new PickupDelivery
            {
                Order = order,
                IsPickup = true,
                AddressId = addressId,
                Status = DeliveryStatus.Scheduled,
                ScheduledAtUtc = request.PreferredPickupAtUtc
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(100000, 999999)}";
}
