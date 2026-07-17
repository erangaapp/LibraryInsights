using FluentValidation;

namespace Inventory.Service.Application.Commands.CorrectBookDetails;

public class CorrectBookDetailsCommandValidator : 
    AbstractValidator<CorrectBookDetailsCommand>
{
    public CorrectBookDetailsCommandValidator()
    {
        RuleFor(x => x.BookId).GreaterThan(0);
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.Author).MaximumLength(100);
        RuleFor(x => x.Isbn).MaximumLength(20);
        RuleFor(x => x.Pages).GreaterThan(0).When(x => x.Pages.HasValue);
        RuleFor(x => x.TotalCopies).GreaterThanOrEqualTo(0).When(x => x.TotalCopies.HasValue);

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Title)
                    || !string.IsNullOrWhiteSpace(x.Author)
                    || !string.IsNullOrWhiteSpace(x.Isbn)
                    || x.Pages.HasValue
                    || x.TotalCopies.HasValue)
            .WithMessage("At least one field to correct must be provided.");
    }
}
