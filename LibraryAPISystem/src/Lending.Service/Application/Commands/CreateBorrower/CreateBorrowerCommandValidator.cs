using FluentValidation;

namespace Lending.Service.Application.Commands.CreateBorrower;

public class CreateBorrowerCommandValidator : AbstractValidator<CreateBorrowerCommand>
{
    public CreateBorrowerCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}
