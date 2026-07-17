using FluentValidation;

namespace Lending.Service.Application.Commands.BorrowBook;

public class BorrowBookCommandValidator : 
    AbstractValidator<BorrowBookCommand>
{
    public BorrowBookCommandValidator()
    {
        RuleFor(x => x.BookId).GreaterThan(0);
        RuleFor(x => x.BorrowerId).GreaterThan(0);
    }
}
