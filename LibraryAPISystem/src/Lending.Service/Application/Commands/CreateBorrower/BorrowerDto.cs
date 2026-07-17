namespace Lending.Service.Application.Commands.CreateBorrower;

public record BorrowerDto(int Id,
    string FirstName, string LastName,
    string Email);
