using Lending.Service.Application.Abstractions;
using Lending.Service.Application.Models;
using Lending.Service.Application.Models.Responses;
using Lending.Service.Extensions;
using Lending.Service.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Application.Queries.GetMostBorrowedBooks;

public class GetMostBorrowedBooksQueryHandler(LendingDbContext db, IBookCatalog catalog)
    : IRequestHandler<GetMostBorrowedBooksQuery, PagedResult<MostBorrowedBookDto>>
{
    private const string UnknownTitle = "Unknown title";

    public async Task<PagedResult<MostBorrowedBookDto>> Handle(
        GetMostBorrowedBooksQuery query, CancellationToken ct)
    {
        // 1. Aggregate in OWN database — grouping, ordering, pagination all in SQL
        var loans = db.Loans.AsNoTracking();

        if (query.From is not null) loans = loans.Where(l => l.BorrowedAt >= query.From);
        if (query.To is not null) loans = loans.Where(l => l.BorrowedAt < query.To);

        var pageResult = await loans
            .GroupBy(l => l.BookId)
            .Select(g => new { BookId = g.Key, BorrowCount = g.Count() })
            .OrderByDescending(x => x.BorrowCount)
            .ThenBy(x => x.BookId) // deterministic tie-break
            .ToPagedResultAsync(query.Page, query.PageSize, ct);

        // 2. Enrich the page (only page-sized id list crosses the wire)
        var books = await catalog.GetBooksAsync(
            [.. pageResult.Items.Select(x => x.BookId)], ct);

        var items = pageResult.Items.Select(x =>
            books.TryGetValue(x.BookId, out var b)
                ? new MostBorrowedBookDto(x.BookId, b.Title, b.Author, x.BorrowCount)
                : new MostBorrowedBookDto(x.BookId, CatalogBook.Unknown(x.BookId).Title, string.Empty, x.BorrowCount))
            .ToList();

        return new PagedResult<MostBorrowedBookDto>(
            items, pageResult.Page, pageResult.PageSize, pageResult.TotalCount);
    }
}
