using FluentValidation.TestHelper;
using Lending.Service.Application.Commands.ReturnLoan;

namespace Lending.Service.Tests.Application;

public class ReturnLoanCommandValidatorTests
{
    private readonly ReturnLoanCommandValidator _validator = new();

    [Fact]
    public void LoanId_WhenGreaterThanZero_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new ReturnLoanCommand(LoanId: 42);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LoanId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void LoanId_WhenZeroOrNegative_ShouldHaveValidationError(int invalidLoanId)
    {
        // Arrange
        var command = new ReturnLoanCommand(LoanId: invalidLoanId);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LoanId);
    }
}
