using Inventory.Service.Application.Models.Responses;
using Inventory.Service.Extensions;
using Inventory.Service.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Application.Queries.GetBooks;

public class GetBooksQueryHandler(InventoryDbContext db)
    : IRequestHandler<GetBooksQuery, PagedResult<BookDto>>
{
    public async Task<PagedResult<BookDto>> Handle(
        GetBooksQuery query, CancellationToken ct)
    {
        var books = db.Books.AsNoTracking();
        if (!query.IncludeDiscontinued)
            books = books.Where(b => b.DiscontinuedDate == null);

        return await books
            .OrderBy(b => b.Title)
            .Select(b => new BookDto(
                b.Id,
                b.Title,
                b.Author,
                b.Isbn,
                b.Pages,
                b.TotalCopies,
                b.DateReceived,
                b.DiscontinuedDate != null))
            .ToPagedResultAsync(query.Page, query.PageSize, ct);
    }
}
