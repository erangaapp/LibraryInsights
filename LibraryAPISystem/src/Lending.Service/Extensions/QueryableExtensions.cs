using Lending.Service.Application.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Extensions;

public static class QueryableExtensions
{
    /// <summary>Executes count + page fetch and wraps the result in the
    /// standard pagination envelope. Compose after filters, ordering,
    /// and projection so both SQL statements stay minimal.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int page, int pageSize, CancellationToken ct)
    {
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>(items, page, pageSize, totalCount);
    }
}
