using Lending.Service.Application.Abstractions;
using Lending.Service.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Application.Queries.GetReadingPace;

public class GetReadingPaceQueryHandler(
    LendingDbContext db,
    IBookCatalog catalog)
    : IRequestHandler<GetReadingPaceQuery, ReadingPaceDto?>
{
    public async Task<ReadingPaceDto?> Handle(
        GetReadingPaceQuery query, CancellationToken ct)
    {
        var borrower = await db.Borrowers.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == query.BorrowerId, ct);

        if (borrower is null)
            return null; // endpoint translates to 404

        // Returned loans only — materialize, then use domain's ReadingDays
        var returnedLoans = await db.Loans.AsNoTracking()
            .Where(l => l.BorrowerId == query.BorrowerId && l.ReturnedAt != null)
            .ToListAsync(ct);

        if (returnedLoans.Count == 0)
            return new ReadingPaceDto(borrower.Id, borrower.FullName, null, []);

        var books = await catalog.GetBooksAsync(
            [.. returnedLoans.Select(l => l.BookId).Distinct()], ct);

        var paceBooks = returnedLoans
            .Where(l => books.ContainsKey(l.BookId))
            .Select(l =>
            {
                var book = books[l.BookId];
                var days = l.ReadingDays!.Value;
                return new ReadingPaceBookDto(
                    l.BookId, book.Title, book.Pages, days,
                    Math.Round((double)book.Pages / days, 1));
            })
            .OrderByDescending(b => b.PagesPerDay)
            .ToList();

        double? average = paceBooks.Count > 0
            ? Math.Round(paceBooks.Average(b => b.PagesPerDay), 1)
            : null;

        return new ReadingPaceDto(borrower.Id, borrower.FullName, average, paceBooks);
    }
}
