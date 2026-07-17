using FluentValidation;

namespace Lending.Service.Application.Commands.ReturnLoan;

public class ReturnLoanCommandValidator : AbstractValidator<ReturnLoanCommand>
{
    public ReturnLoanCommandValidator()
    {
        RuleFor(x => x.LoanId).GreaterThan(0);
    }
}