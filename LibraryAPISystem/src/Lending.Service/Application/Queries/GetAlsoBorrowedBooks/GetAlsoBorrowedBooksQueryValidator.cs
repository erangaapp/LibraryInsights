using FluentValidation;

namespace Lending.Service.Application.Queries.GetAlsoBorrowedBooks;

public class GetAlsoBorrowedBooksQueryValidator : 
    AbstractValidator<GetAlsoBorrowedBooksQuery>
{
    public GetAlsoBorrowedBooksQueryValidator()
    {
        RuleFor(x => x.BookId).GreaterThan(0);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
