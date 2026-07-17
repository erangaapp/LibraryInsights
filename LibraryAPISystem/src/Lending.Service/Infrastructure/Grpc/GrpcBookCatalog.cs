using Grpc.Core;
using Lending.Service.Application.Abstractions;
using Lending.Service.Application.Models;
using Lending.Service.Extensions;
using Library.Contracts.Inventory;

namespace Lending.Service.Infrastructure.Grpc;

public class GrpcBookCatalog(
    InventoryGrpc.InventoryGrpcClient client,
    ILogger<GrpcBookCatalog> logger) : IBookCatalog
{
    public async Task<CatalogBook?> GetBookAsync(int bookId, CancellationToken ct)
    {
        if (bookId <= 0)
            return null;

        try
        {
            var response = await client.GetBookByIdAsync(
                new GetBookByIdRequest { BookId = bookId },
                cancellationToken: ct);

            return response.Book.ToCatalog();
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null; // missing book → caller's 404 policy
        }

        //No need extra hanlding for Unavailable/DeadlineExceeded, because the caller will handle it.
    }

    public async Task<IReadOnlyDictionary<int, CatalogBook>> GetBooksAsync(
        IReadOnlyCollection<int> bookIds, CancellationToken ct)
    {
        if (bookIds.Count == 0)
            return new Dictionary<int, CatalogBook>();

        try
        {
            var request = new GetBooksByIdsRequest();
            request.BookIds.AddRange(bookIds);

            var response = await client.
                GetBooksByIdsAsync(request, cancellationToken: ct);

            return response.Books.
                ToDictionary(b => b.Id, b => b.ToCatalog());
        }
        catch (RpcException ex) when
            (ex.StatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded)
        {
            // Partial-failure policy for READS: degrade, don't fail the insight.
            // Missing entries render as "Unknown title" downstream.
            logger.LogWarning(ex,
                "Inventory service unreachable; returning empty catalog for {Count} book ids",
                bookIds.Count);
            return new Dictionary<int, CatalogBook>();
        }
    }
}
