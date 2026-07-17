using FluentValidation;

namespace Lending.Service.Application.Queries.GetReadingPace;

public class GetReadingPaceQueryValidator : 
    AbstractValidator<GetReadingPaceQuery>
{
    public GetReadingPaceQueryValidator()
    {
        RuleFor(x => x.BorrowerId)
            .GreaterThan(0)
            .WithMessage("borrowerId must be a positive integer.");
    }
}
