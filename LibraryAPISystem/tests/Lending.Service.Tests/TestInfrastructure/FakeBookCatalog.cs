using Lending.Service.Application.Abstractions;
using Lending.Service.Application.Models;

namespace Lending.Service.Tests.TestInfrastructure;

public sealed class FakeBookCatalog(params CatalogBook[] books) : IBookCatalog
{
    private readonly Dictionary<int, CatalogBook> _books =
        books.ToDictionary(b => b.Id);

    public Task<IReadOnlyDictionary<int, CatalogBook>> GetBooksAsync(
        IReadOnlyCollection<int> bookIds, CancellationToken ct) => 
        Task.FromResult<IReadOnlyDictionary<int, CatalogBook>>(bookIds.Where(_books.ContainsKey)
                   .ToDictionary(id => id, id => _books[id]));

    public Task<CatalogBook?> GetBookAsync(int bookId, CancellationToken ct) =>
        Task.FromResult(_books.GetValueOrDefault(bookId));
}
