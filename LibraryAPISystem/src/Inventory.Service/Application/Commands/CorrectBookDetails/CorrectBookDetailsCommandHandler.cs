using Inventory.Service.Application.Exceptions;
using Inventory.Service.Application.Queries.GetBooks;
using Inventory.Service.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Application.Commands.CorrectBookDetails;

public class CorrectBookDetailsCommandHandler(InventoryDbContext db)
    : IRequestHandler<CorrectBookDetailsCommand, BookDto>
{
    public async Task<BookDto> Handle(CorrectBookDetailsCommand cmd, CancellationToken ct)
    {
        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == cmd.BookId, ct)
            ?? throw new NotFoundException($"No book exists with id {cmd.BookId}.");

        // Database coordination (conflict checking)
        if (!string.IsNullOrWhiteSpace(cmd.Isbn) && cmd.Isbn != book.Isbn)
        {
            if (await db.Books.AnyAsync(b => b.Isbn == cmd.Isbn && b.Id != cmd.BookId, ct))
                throw new ConflictException($"Another book already has ISBN {cmd.Isbn}.");
        }

        // Domain business boundary
        book.CorrectDetails(
            title: cmd.Title,
            author: cmd.Author,
            isbn: cmd.Isbn,
            pages: cmd.Pages,
            totalCopies: cmd.TotalCopies
        );

        await db.SaveChangesAsync(ct);

        return new BookDto(
            book.Id,
            book.Title,
            book.Author,
            book.Isbn,
            book.Pages,
            book.TotalCopies,
            book.DateReceived,
            book.DiscontinuedDate is null
        );
    }
}
