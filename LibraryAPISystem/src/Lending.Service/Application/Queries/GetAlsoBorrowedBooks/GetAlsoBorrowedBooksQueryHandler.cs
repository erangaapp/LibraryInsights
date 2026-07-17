using Lending.Service.Application.Abstractions;
using Lending.Service.Application.Models;
using Lending.Service.Application.Models.Responses;
using Lending.Service.Extensions;
using Lending.Service.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Application.Queries.GetAlsoBorrowedBooks;

public class GetAlsoBorrowedBooksQueryHandler(LendingDbContext db, IBookCatalog catalog)
    : IRequestHandler<GetAlsoBorrowedBooksQuery, PagedResult<AlsoBorrowedBookDto>>
{
    public async Task<PagedResult<AlsoBorrowedBookDto>> Handle(
        GetAlsoBorrowedBooksQuery query, CancellationToken ct)
    {
        // Borrowers of the seed book (stays IQueryable — composes into one SQL statement)
        var borrowerIds = db.Loans.AsNoTracking()
            .Where(l => l.BookId == query.BookId)
            .Select(l => l.BorrowerId);

        // Their OTHER books, ranked by distinct shared borrowers
        var counts = await db.Loans.AsNoTracking()
            .Where(l => l.BookId != query.BookId && borrowerIds.Contains(l.BorrowerId))
            .GroupBy(l => l.BookId)
            .Select(g => new
            {
                BookId = g.Key,
                SharedBorrowerCount = g.Select(l => l.BorrowerId).Distinct().Count(),
            })
            .OrderByDescending(x => x.SharedBorrowerCount)
            .ThenBy(x => x.BookId)
            .ToPagedResultAsync(query.Page, query.PageSize, ct);

        var books = await catalog.GetBooksAsync(
            [.. counts.Items.Select(x => x.BookId)], ct);

        var items = counts.Items.Select(x =>
        {
            var book = books.TryGetValue(x.BookId, out var b) ? b : CatalogBook.Unknown(x.BookId);
            return new AlsoBorrowedBookDto(x.BookId, book.Title, book.Author, x.SharedBorrowerCount);
        }).ToList();

        return new PagedResult<AlsoBorrowedBookDto>(
            items, counts.Page, counts.PageSize, counts.TotalCount);
    }
}
