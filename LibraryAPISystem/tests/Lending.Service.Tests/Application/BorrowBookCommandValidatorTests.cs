using FluentValidation.TestHelper;
using Lending.Service.Application.Commands.BorrowBook;

namespace Lending.Service.Tests.Application;

public class BorrowBookCommandValidatorTests
{
    private readonly BorrowBookCommandValidator _validator = new();

    [Fact]
    public void Validator_WhenBothIdsAreValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new BorrowBookCommand(BorrowerId: 10, BookId: 101);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BorrowerId);
        result.ShouldNotHaveValidationErrorFor(x => x.BookId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BookId_WhenZeroOrNegative_ShouldHaveValidationError(int invalidBookId)
    {
        // Arrange
        var command = new BorrowBookCommand(BorrowerId: 5, BookId: invalidBookId);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.BookId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BorrowerId_WhenZeroOrNegative_ShouldHaveValidationError(int invalidBorrowerId)
    {
        // Arrange
        var command = new BorrowBookCommand(BorrowerId: invalidBorrowerId, BookId: 52);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.BorrowerId);
    }
}
