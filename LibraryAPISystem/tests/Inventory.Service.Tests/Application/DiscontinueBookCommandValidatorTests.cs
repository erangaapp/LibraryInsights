using FluentValidation.TestHelper;
using Inventory.Service.Application.Commands.DiscontinueBook;

namespace Inventory.Service.Tests.Application;

public class DiscontinueBookCommandValidatorTests
{
    private readonly DiscontinueBookCommandValidator _validator = new();

    [Fact]
    public void PositiveBookId_Passes()
    {
        _validator.TestValidate(new DiscontinueBookCommand(1))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveBookId_Fails(int bookId)
    {
        _validator.TestValidate(new DiscontinueBookCommand(bookId))
            .ShouldHaveValidationErrorFor(x => x.BookId);
    }
}
