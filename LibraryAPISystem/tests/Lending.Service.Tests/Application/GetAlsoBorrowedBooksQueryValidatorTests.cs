using FluentValidation.TestHelper;
using Lending.Service.Application.Queries.GetAlsoBorrowedBooks;

namespace Lending.Service.Tests.Application;

public class GetAlsoBorrowedBooksQueryValidatorTests
{
    private readonly GetAlsoBorrowedBooksQueryValidator _validator = new();

    [Fact]
    public void Validator_WhenAllValuesAreValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var query = new GetAlsoBorrowedBooksQuery(BookId: 42, Page: 1, PageSize: 20);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BookId);
        result.ShouldNotHaveValidationErrorFor(x => x.Page);
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BookId_WhenZeroOrNegative_ShouldHaveValidationError(int invalidBookId)
    {
        // Arrange
        var query = new GetAlsoBorrowedBooksQuery(BookId: invalidBookId, Page: 1, PageSize: 10);

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.BookId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Page_WhenLessThanOne_ShouldHaveValidationError(int invalidPage)
    {
        // Arrange
        var query = new GetAlsoBorrowedBooksQuery(BookId: 42, Page: invalidPage, PageSize: 10);

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void PageSize_WhenWithinRange_ShouldNotHaveValidationError(int validPageSize)
    {
        // Arrange
        var query = new GetAlsoBorrowedBooksQuery(BookId: 42, Page: 1, PageSize: validPageSize);

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void PageSize_WhenOutsideRange_ShouldHaveValidationError(int invalidPageSize)
    {
        // Arrange
        var query = new GetAlsoBorrowedBooksQuery(BookId: 42, Page: 1, PageSize: invalidPageSize);

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}
