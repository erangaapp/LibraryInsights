using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Inventory.Service.Application.Queries.GetBookById;
using Inventory.Service.Application.Queries.GetBooksByIds;
using Library.Contracts.Inventory;
using Library.Contracts.Inventory.Common;
using MediatR;

namespace Inventory.Service.Application.Grpc;

public class InventoryGrpcService(ISender sender)
    : InventoryGrpc.InventoryGrpcBase
{
    public override async Task<GetBooksByIdsResponse> GetBooksByIds(
        GetBooksByIdsRequest request, ServerCallContext context)
    {
        var books = await sender.Send(
            new GetBooksByIdsQuery([.. request.BookIds]),
            context.CancellationToken);

        var response = new GetBooksByIdsResponse();
        response.Books.AddRange(books.Select(b => new BookInfo
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            Pages = b.Pages
        }));
        return response;
    }

    public override async Task<GetBookByIdResponse> GetBookById(
        GetBookByIdRequest request, ServerCallContext context)
    {
        var book = await sender.Send(
            new GetBookByIdQuery(request.BookId),
            context.CancellationToken);

        return book == null ? 
            throw new RpcException(new Status(StatusCode.NotFound, "Book not found"))
            : new GetBookByIdResponse
            {
                Book = new Book
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Isbn = book.Isbn,
                    Pages = book.Pages,
                    Copies = book.TotalCopies,
                    DateReceived = Timestamp.FromDateTime(
                        book.DateReceived.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)),
                    IsDiscontinued = book.IsDiscontinued,
                }
            };
    }
}