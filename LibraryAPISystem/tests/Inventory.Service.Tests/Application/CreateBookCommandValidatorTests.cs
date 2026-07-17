using FluentValidation.TestHelper;
using Inventory.Service.Application.Commands.CreateBook;

namespace Inventory.Service.Tests.Application;

public class CreateBookCommandValidatorTests
{
    private readonly CreateBookCommandValidator _validator = new();

    private static CreateBookCommand Valid() =>
        new("The Hobbit", "J.R.R. Tolkien", "9780345339683", 310, 3);

    [Fact]
    public void ValidCommand_Passes()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void BlankTitle_Fails(string title)
    {
        var result = _validator.TestValidate(Valid() with { Title = title });
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void TitleTooLong_Fails()
    {
        var result = _validator.TestValidate(Valid() with { Title = new string('x', 201) });
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void BlankIsbn_Fails()
    {
        var result = _validator.TestValidate(Valid() with { Isbn = "" });
        result.ShouldHaveValidationErrorFor(x => x.Isbn);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void NonPositivePages_Fails(int pages)
    {
        var result = _validator.TestValidate(Valid() with { Pages = pages });
        result.ShouldHaveValidationErrorFor(x => x.Pages);
    }

    [Fact]
    public void NegativeCopies_Fails()
    {
        var result = _validator.TestValidate(Valid() with { TotalCopies = -1 });
        result.ShouldHaveValidationErrorFor(x => x.TotalCopies);
    }
}
