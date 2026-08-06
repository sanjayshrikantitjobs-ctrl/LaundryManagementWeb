using FluentAssertions;
using LaundryMgmt.Application.Customers.Commands.CreateCustomer;
using Xunit;

namespace LaundryMgmt.UnitTests.Application;

public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_PassesValidation()
    {
        var command = new CreateCustomerCommand("Jane Doe", "9876543210", "jane@example.com", 500, null, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Missing_FullName_FailsValidation()
    {
        var command = new CreateCustomerCommand("", "9876543210", null, 0, null, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerCommand.FullName));
    }

    [Fact]
    public void Invalid_Email_FailsValidation()
    {
        var command = new CreateCustomerCommand("Jane Doe", "9876543210", "not-an-email", 0, null, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerCommand.Email));
    }

    [Fact]
    public void Negative_CreditLimit_FailsValidation()
    {
        var command = new CreateCustomerCommand("Jane Doe", "9876543210", null, -1, null, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerCommand.CreditLimit));
    }

    [Fact]
    public void Address_Missing_Line1_FailsValidation()
    {
        var command = new CreateCustomerCommand(
            "Jane Doe", "9876543210", null, 0, null,
            new List<CreateCustomerAddressDto> { new("Home", "", null, "City", "State", "12345", true) });

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
