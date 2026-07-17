using FluentValidation.TestHelper;
using Lending.Service.Application.Queries.GetTopBorrowers;

namespace Lending.Service.Tests.Application;

public class GetTopBorrowersQueryValidatorTests
{
    private readonly GetTopBorrowersQueryValidator _validator = new();

    [Fact]
    public void Validator_WhenAllValuesAreValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var query = new GetTopBorrowersQuery(
            Page: 1,
            PageSize: 10,
            From: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            To: new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
        );

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Page_WhenLessThanOne_ShouldHaveValidationError(int invalidPage)
    {
        // Arrange
        var query = new GetTopBorrowersQuery
            (From: null, To: null, Page: invalidPage, PageSize: 10);

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void PageSize_WhenOutsideRange_ShouldHaveValidationError(int invalidPageSize)
    {
        // Arrange
        var query = new GetTopBorrowersQuery
            (From: null, To: null, Page: 1, PageSize: invalidPageSize);

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(null, null)]                                                 // Both null
    [InlineData("2026-01-01T00:00:00Z", null)]                               // Only From set
    [InlineData(null, "2026-01-01T00:00:00Z")]                               // Only To set
    [InlineData("2026-01-01T12:00:00Z", "2026-01-01T12:00:00Z")]             // From equals To
    [InlineData("2026-01-01T08:00:00Z", "2026-01-01T17:00:00Z")]             // From earlier than To
    public void Dates_WhenValidCombinations_ShouldNotHaveValidationError(string? fromStr, string? toStr)
    {
        // Arrange
        DateTime? from = fromStr is not null ? DateTime.Parse(fromStr, null, System.Globalization.DateTimeStyles.RoundtripKind) : null;
        DateTime? to = toStr is not null ? DateTime.Parse(toStr, null, System.Globalization.DateTimeStyles.RoundtripKind) : null;

        var query = new GetTopBorrowersQuery(Page: 1, PageSize: 10, From: from, To: to);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Dates_WhenFromIsLaterThanTo_ShouldHaveValidationError()
    {
        // Arrange
        var query = new GetTopBorrowersQuery(
            Page: 1,
            PageSize: 10,
            From: new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc), // Later
            To: new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)      // Earlier
        );

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage("'from' must be earlier than or equal to 'to'.");
    }
}
