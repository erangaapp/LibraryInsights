using FluentValidation;

namespace Lending.Service.Application.Queries.GetMostBorrowedBooks;

public class GetMostBorrowedBooksQueryValidator : 
    AbstractValidator<GetMostBorrowedBooksQuery>
{
    public GetMostBorrowedBooksQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x)
            .Must(x => x.From is null || x.To is null || x.From <= x.To)
            .WithMessage("'from' must be earlier than or equal to 'to'.");
    }
}
