using Lending.Service.Application.Abstractions;
using Lending.Service.Application.Exceptions;
using Lending.Service.Domain.Entities;
using Lending.Service.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Application.Commands.BorrowBook;

public class BorrowBookCommandHandler(LendingDbContext db, IBookCatalog catalog)
    : IRequestHandler<BorrowBookCommand, LoanDto>
{
    public async Task<LoanDto> Handle(BorrowBookCommand cmd, CancellationToken ct)
    {
        if (!await db.Borrowers.AnyAsync(b => b.Id == cmd.BorrowerId, ct))
            throw new NotFoundException($"No borrower exists with id {cmd.BorrowerId}.");

        // Strict lookup — RpcException propagates → 503 (fail closed on writes)
        var book = await catalog.GetBookAsync(cmd.BookId, ct)
            ?? throw new NotFoundException($"No book exists with id {cmd.BookId}.");

        if (book.IsDiscontinued)
            throw new ConflictException($"Book {cmd.BookId} is discontinued and cannot be borrowed.");

        var totalCopies = book.Copies ?? 0;
        var activeLoansCount = await db.Loans
            .CountAsync(l => l.BookId == cmd.BookId && l.ReturnedAt == null, ct);

        if (activeLoansCount >= totalCopies)
            throw new ConflictException($"All {totalCopies} copies of book {cmd.BookId} are currently on loan.");

        var loan = new Loan
        {
            BookId = cmd.BookId,
            BorrowerId = cmd.BorrowerId,
            BorrowedAt = DateTime.UtcNow,
        };
        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        return new LoanDto(loan.Id, loan.BookId, loan.BorrowerId, loan.BorrowedAt, loan.ReturnedAt);
    }
}
