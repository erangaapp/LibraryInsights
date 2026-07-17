namespace Lending.Service.Application.Commands.BorrowBook;

public record LoanDto(int Id, int BookId, int BorrowerId,
    DateTime BorrowedAt, DateTime? ReturnedAt);
