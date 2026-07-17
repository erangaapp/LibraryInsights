namespace Lending.Service.Application.Queries.GetTopBorrowers;

public record TopBorrowerDto(int BorrowerId,
    string FirstName, string? LastName,
    int BorrowCount);
