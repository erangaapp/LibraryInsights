using Inventory.Service.Application.Queries.GetBooks;
using MediatR;

namespace Inventory.Service.Application.Commands.CreateBook;

public record CreateBookCommand(string Title, string Author, string Isbn,
    int Pages, int TotalCopies) : IRequest<BookDto>;
