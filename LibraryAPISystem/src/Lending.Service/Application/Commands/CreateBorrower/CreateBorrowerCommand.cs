using MediatR;

namespace Lending.Service.Application.Commands.CreateBorrower;

public record CreateBorrowerCommand(string FirstName, string LastName, string Email) 
    : IRequest<BorrowerDto>;
