using Inventory.Service.Application.Exceptions;
using Inventory.Service.Application.Queries.GetBooks;
using Inventory.Service.Domain.Entities;
using Inventory.Service.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Application.Commands.CreateBook;

public class CreateBookCommandHandler(InventoryDbContext db)
    : IRequestHandler<CreateBookCommand, BookDto>
{
    public async Task<BookDto> Handle(CreateBookCommand cmd, CancellationToken ct)
    {
        // Business Validation (External to the Entity)
        if (await db.Books.AnyAsync(b => b.Isbn == cmd.Isbn, ct))
            throw new ConflictException($"A book with ISBN {cmd.Isbn} already exists.");

        // Domain business
        var book = new Book(
            title: cmd.Title.Trim(),
            author: cmd.Author.Trim(),
            isbn: cmd.Isbn.Trim(),
            pages: cmd.Pages,
            dateReceived: DateOnly.FromDateTime(DateTime.UtcNow),
            totalCopies: cmd.TotalCopies
        );

        //DB execution
        db.Books.Add(book);
        await db.SaveChangesAsync(ct);

        return new BookDto(
            book.Id,
            book.Title,
            book.Author,
            book.Isbn,
            book.Pages,
            book.TotalCopies,
            book.DateReceived
        );
    }
}
