using FluentValidation.TestHelper;
using Inventory.Service.Application.Commands.CorrectBookDetails;

namespace Inventory.Service.Tests.Application;

public class CorrectBookDetailsCommandValidatorTests
{
    private readonly CorrectBookDetailsCommandValidator _validator = new();

    [Fact]
    public void ValidSingleFieldCorrection_Passes()
    {
        var result = _validator.TestValidate(
            new CorrectBookDetailsCommand
            (1, "The Hobbit", null, null, null, null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void NoFieldsProvided_Fails()
    {
        var result = _validator.TestValidate(
            new CorrectBookDetailsCommand
            (1, null, null, null, null, null));

        result.ShouldHaveValidationErrors().
            WithErrorMessage("At least one field to correct must be provided.");
    }

    [Fact]
    public void InvalidBookId_Fails()
    {
        var result = _validator.TestValidate(
            new CorrectBookDetailsCommand
            (0, "Title", null, null, null, null));

        result.ShouldHaveValidationErrorFor(x => x.BookId);
    }

    [Fact]
    public void TitleTooLong_Fails()
    {
        var result = _validator.TestValidate(
            new CorrectBookDetailsCommand
            (1, new string('x', 201), null, null, null, null));

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void NonPositivePages_Fails(int pages)
    {
        var result = _validator.TestValidate(
            new CorrectBookDetailsCommand
            (1, null, null, null, pages, null));

        result.ShouldHaveValidationErrorFor(x => x.Pages);
    }

    [Fact]
    public void NegativeCopies_Fails()
    {
        var result = _validator.TestValidate(
            new CorrectBookDetailsCommand
            (1, null, null, null, null, -1));

        result.ShouldHaveValidationErrorFor(x => x.TotalCopies);
    }
}
