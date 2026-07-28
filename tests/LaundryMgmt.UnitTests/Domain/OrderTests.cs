using FluentAssertions;
using LaundryMgmt.Domain.Entities;
using LaundryMgmt.Domain.Enums;
using LaundryMgmt.Domain.Exceptions;
using Xunit;

namespace LaundryMgmt.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void AdvanceTo_NextStepInSequence_Succeeds()
    {
        var order = new Order { OrderNumber = "ORD-2026-000001" };

        order.AdvanceTo(OrderStatus.Received, changedBy: "tester");

        order.Status.Should().Be(OrderStatus.Received);
        order.StatusHistory.Should().ContainSingle(h => h.Status == OrderStatus.Received);
    }

    [Fact]
    public void AdvanceTo_SkippingAStep_ThrowsDomainException()
    {
        var order = new Order { OrderNumber = "ORD-2026-000002" };

        var act = () => order.AdvanceTo(OrderStatus.Washing); // skips Received, Sorting

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AdvanceTo_Cancelled_AllowedFromAnyNonTerminalState()
    {
        var order = new Order { OrderNumber = "ORD-2026-000003" };
        order.AdvanceTo(OrderStatus.Received);

        var act = () => order.AdvanceTo(OrderStatus.Cancelled);

        act.Should().NotThrow();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void AdvanceTo_AfterDelivered_ThrowsDomainException()
    {
        var order = new Order { OrderNumber = "ORD-2026-000004" };
        foreach (var status in new[]
        {
            OrderStatus.Received, OrderStatus.Sorting, OrderStatus.Washing, OrderStatus.Drying,
            OrderStatus.Ironing, OrderStatus.Packing, OrderStatus.ReadyForDelivery, OrderStatus.Delivered
        })
        {
            order.AdvanceTo(status);
        }

        var act = () => order.AdvanceTo(OrderStatus.Cancelled);

        act.Should().Throw<DomainException>();
    }
}
