using Inventory.Service.Application.Exceptions;
using Inventory.Service.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Application.Commands.DiscontinueBook;

public class DiscontinueBookCommandHandler(InventoryDbContext db)
    : IRequestHandler<DiscontinueBookCommand>
{
    public async Task Handle(DiscontinueBookCommand cmd, CancellationToken ct)
    {
        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == cmd.BookId, ct)
            ?? throw new NotFoundException($"No book exists with id {cmd.BookId}.");

        if (book.DiscontinuedDate is not null)
            throw new ConflictException($"Book {cmd.BookId} is already discontinued.");

        // Execute domain boundary
        book.Discontinue(DateOnly.FromDateTime(DateTime.UtcNow));

        await db.SaveChangesAsync(ct);
    }
}