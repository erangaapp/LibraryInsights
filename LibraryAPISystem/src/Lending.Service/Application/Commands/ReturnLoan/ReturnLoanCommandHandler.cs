using Lending.Service.Application.Commands.BorrowBook;
using Lending.Service.Application.Exceptions;
using Lending.Service.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Application.Commands.ReturnLoan;

public class ReturnLoanCommandHandler(LendingDbContext db)
    : IRequestHandler<ReturnLoanCommand, LoanDto>
{
    public async Task<LoanDto> Handle(ReturnLoanCommand cmd, CancellationToken ct)
    {
        var loan = await db.Loans.FirstOrDefaultAsync(l => l.Id == cmd.LoanId, ct)
            ?? throw new NotFoundException($"No loan exists with id {cmd.LoanId}.");

        if (loan.IsReturned)
            throw new ConflictException($"Loan {cmd.LoanId} was already returned on {loan.ReturnedAt:u}.");

        loan.ReturnedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return new LoanDto(loan.Id, loan.BookId,
            loan.BorrowerId, loan.BorrowedAt,
            loan.ReturnedAt);
    }
}
