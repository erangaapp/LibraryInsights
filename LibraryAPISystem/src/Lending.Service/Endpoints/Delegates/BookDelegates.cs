using Lending.Service.Application.Queries.GetAlsoBorrowedBooks;
using Lending.Service.Application.Queries.GetMostBorrowedBooks;
using MediatR;

namespace Lending.Service.Endpoints.Delegates;

internal static class BookDelegates
{
    public static async Task<IResult> GetMostBorrowedBooksAsync(
        ISender sender, CancellationToken ct,
        DateTime? from = null, DateTime? to = null,
        int page = 1, int pageSize = 10)
    {
        return Results.Ok(await sender.Send(
            new GetMostBorrowedBooksQuery(from, to, page, pageSize), ct));
    }

    public static async Task<IResult> GetAlsoBorrowedBooksAsync(
        int bookId, ISender sender, CancellationToken ct,
        int page = 1, int pageSize = 10)
    {
        return Results.Ok(await sender.Send(
            new GetAlsoBorrowedBooksQuery(bookId, page, pageSize), ct));
    }
}
