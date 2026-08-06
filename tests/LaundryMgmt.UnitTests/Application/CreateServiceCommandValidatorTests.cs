using FluentAssertions;
using LaundryMgmt.Application.Services.Commands.CreateService;
using Xunit;

namespace LaundryMgmt.UnitTests.Application;

public class CreateServiceCommandValidatorTests
{
    private readonly CreateServiceCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_PassesValidation()
    {
        var command = new CreateServiceCommand("Wash & Iron", 50, 24, 5, 1);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void GstPercentage_OutOfRange_FailsValidation(decimal gst)
    {
        var command = new CreateServiceCommand("Wash & Iron", 50, 24, gst, 1);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateServiceCommand.GstPercentage));
    }

    [Fact]
    public void Zero_EstimatedTimeHours_FailsValidation()
    {
        var command = new CreateServiceCommand("Wash & Iron", 50, 0, 5, 1);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateServiceCommand.EstimatedTimeHours));
    }

    [Fact]
    public void Negative_BasePrice_FailsValidation()
    {
        var command = new CreateServiceCommand("Wash & Iron", -1, 24, 5, 1);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateServiceCommand.BasePrice));
    }
}
