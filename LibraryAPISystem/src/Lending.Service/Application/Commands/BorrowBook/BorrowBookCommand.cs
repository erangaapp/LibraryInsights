using MediatR;

namespace Lending.Service.Application.Commands.BorrowBook;

public record BorrowBookCommand(int BookId, int BorrowerId) 
    : IRequest<LoanDto>;
