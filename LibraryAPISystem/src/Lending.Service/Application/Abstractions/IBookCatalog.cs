using Lending.Service.Application.Models;

namespace Lending.Service.Application.Abstractions;

public interface IBookCatalog
{
    /// <summary>Batch lookup of book details from the Inventory service.
    /// Returns a dictionary keyed by book id; ids unknown to the catalog
    /// are absent, missing-book presentation policy.</summary>
    Task<IReadOnlyDictionary<int, CatalogBook>> GetBooksAsync(
        IReadOnlyCollection<int> bookIds, CancellationToken ct);

    /// <summary>Strict single lookup for write paths. Propagates transport
    /// failures (callers fail closed); null = book does not exist.</summary>
    Task<CatalogBook?> GetBookAsync(int bookId, CancellationToken ct);
}
