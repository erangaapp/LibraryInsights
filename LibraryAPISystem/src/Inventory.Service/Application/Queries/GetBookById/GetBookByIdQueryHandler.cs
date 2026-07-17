using Inventory.Service.Application.Queries.GetBooks;
using Inventory.Service.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Application.Queries.GetBookById;

public class GetBookByIdQueryHandler(InventoryDbContext db)
    : IRequestHandler<GetBookByIdQuery, BookDto?>
{
    public async Task<BookDto?> Handle(
        GetBookByIdQuery query, CancellationToken ct)
    {
        return await db.Books
            .AsNoTracking()
            .Where(b => b.Id == query.Id)
            .Select(b => new BookDto(b.Id,
                b.Title, b.Author, b.Isbn, b.Pages,
                b.TotalCopies, b.DateReceived,
                b.DiscontinuedDate != null))
            .FirstOrDefaultAsync(ct);
    }
}