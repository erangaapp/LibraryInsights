using FluentValidation.TestHelper;
using Lending.Service.Application.Commands.CreateBorrower;

namespace Lending.Service.Tests.Application;

public class CreateBorrowerCommandValidatorTests
{
    private readonly CreateBorrowerCommandValidator _validator = new();

    [Fact]
    public void FirstName_WhenValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateBorrowerCommand("Alice", "Silva", "alice@test.local");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FirstName_WhenEmptyOrNull_ShouldHaveValidationError(string? invalidName)
    {
        // Arrange
        var command = new CreateBorrowerCommand(invalidName!, "Silva", "alice@test.local");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("'First Name' must not be empty.");
    }

    [Fact]
    public void FirstName_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var tooLongName = new string('A', 51);
        var command = new CreateBorrowerCommand(tooLongName, "Silva", "alice@test.local");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void LastName_WhenEmptyOrNull_ShouldHaveValidationError(string? invalidLastName)
    {
        // Arrange
        var command = new CreateBorrowerCommand("Alice", invalidLastName!, "alice@test.local");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("'Last Name' must not be empty.");
    }

    [Fact]
    public void LastName_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var tooLongLastName = new string('B', 51);
        var command = new CreateBorrowerCommand("Alice", tooLongLastName, "alice@test.local");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Email_WhenEmptyOrNull_ShouldHaveValidationError(string? invalidEmail)
    {
        // Arrange
        var command = new CreateBorrowerCommand("Alice", "Silva", invalidEmail!);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@missingusername.com")]
    [InlineData("username@")]
    public void Email_WhenInvalidFormat_ShouldHaveValidationError(string invalidEmail)
    {
        // Arrange
        var command = new CreateBorrowerCommand("Alice", "Silva", invalidEmail);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longEmail = new string('a', 195) + "@test.local";
        var command = new CreateBorrowerCommand("Alice", "Silva", longEmail);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
