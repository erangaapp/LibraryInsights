using Inventory.Service.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Application.Queries.GetBooksByIds;

public class GetBooksByIdsQueryHandler(InventoryDbContext db)
    : IRequestHandler<GetBooksByIdsQuery, IReadOnlyList<BookEnrichmentDto>>
{
    public async Task<IReadOnlyList<BookEnrichmentDto>> Handle(
        GetBooksByIdsQuery query, CancellationToken ct)
    {
        var ids = query.Ids.Distinct().ToArray();
        if (ids.Length == 0)
            return [];

        return await db.Books
            .AsNoTracking()
            .Where(b => ids.Contains(b.Id))
            .Select(b => new BookEnrichmentDto(b.Id, b.Title, b.Author, b.Pages))
            .ToListAsync(ct);
    }
}