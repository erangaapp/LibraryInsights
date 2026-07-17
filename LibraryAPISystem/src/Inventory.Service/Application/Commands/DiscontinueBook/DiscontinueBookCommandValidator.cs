using FluentValidation;

namespace Inventory.Service.Application.Commands.DiscontinueBook;

public class DiscontinueBookCommandValidator : 
    AbstractValidator<DiscontinueBookCommand>
{
    public DiscontinueBookCommandValidator()
    {
        RuleFor(x => x.BookId).GreaterThan(0);
    }
}
