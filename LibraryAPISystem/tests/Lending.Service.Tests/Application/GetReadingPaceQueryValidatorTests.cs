using FluentValidation.TestHelper;
using Lending.Service.Application.Queries.GetReadingPace;

namespace Lending.Service.Tests.Application;

public class GetReadingPaceQueryValidatorTests
{
    private readonly GetReadingPaceQueryValidator _validator = new();

    [Fact]
    public void BorrowerId_WhenGreaterThanZero_ShouldNotHaveValidationError()
    {
        // Arrange
        var query = new GetReadingPaceQuery(BorrowerId: 42);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BorrowerId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void BorrowerId_WhenZeroOrNegative_ShouldHaveValidationError(int invalidBorrowerId)
    {
        // Arrange
        var query = new GetReadingPaceQuery(BorrowerId: invalidBorrowerId);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BorrowerId)
              .WithErrorMessage("borrowerId must be a positive integer.");
    }
}
