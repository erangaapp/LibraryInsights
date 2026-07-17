using FluentValidation;

namespace Inventory.Service.Application.Commands.CreateBook;

public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Author).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Isbn).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Pages).GreaterThan(0);
        RuleFor(x => x.TotalCopies).GreaterThanOrEqualTo(0);
    }
}
