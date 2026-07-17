using Inventory.Service.Application.Commands.CorrectBookDetails;
using Inventory.Service.Application.Commands.CreateBook;
using Inventory.Service.Application.Commands.DiscontinueBook;
using Inventory.Service.Application.Queries.GetBooks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Service.Endpoints.Delegates;

internal static class InventoryEndpointDelegates
{
    public static async Task<IResult> GetBooksAsync(
       ISender sender,
       CancellationToken ct,
       int page = 1,
       int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return Results.Problem(
                title: "Invalid pagination",
                detail: "page must be >= 1 and pageSize between 1 and 100.",
                statusCode: StatusCodes.Status400BadRequest);

        var result = await sender.Send(new GetBooksQuery(page, pageSize), ct);
        return Results.Ok(result);
    }

    public static async Task<IResult> CreateBookAsync(
        CreateBookCommand command, ISender sender, CancellationToken ct)
    {
        var book = await sender.Send(command, ct);
        return Results.Created($"/api/inventory/books/{book.Id}", book);
    }

    public static async Task<IResult> DiscontinueBookAsync(
        int bookId, ISender sender, CancellationToken ct)
    {
        await sender.Send(new DiscontinueBookCommand(bookId), ct);
        return Results.NoContent();
    }

    public static async Task<IResult> CorrectBookDetailsAsync(int bookId,
       [FromBody] CorrectBookDetailsCommand command, ISender sender, CancellationToken ct)
    {
        var book = await sender.Send(command with { BookId = bookId }, ct);
        return Results.Ok(book);
    }
}
