using FluentAssertions;
using LaundryMgmt.Application.Garments.Commands.CreateGarment;
using Xunit;

namespace LaundryMgmt.UnitTests.Application;

public class CreateGarmentCommandValidatorTests
{
    private readonly CreateGarmentCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_PassesValidation()
    {
        var command = new CreateGarmentCommand("Shirt", "Menswear", "SHIRT-001", null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Missing_Name_FailsValidation()
    {
        var command = new CreateGarmentCommand("", "Menswear", null, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateGarmentCommand.Name));
    }

    [Fact]
    public void Missing_Category_FailsValidation()
    {
        var command = new CreateGarmentCommand("Shirt", "", null, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateGarmentCommand.Category));
    }
}
