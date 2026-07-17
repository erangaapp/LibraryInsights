using Inventory.Service.Application.Queries.GetBooks;
using MediatR;

namespace Inventory.Service.Application.Commands.CorrectBookDetails;

/// <summary>Corrects erroneous catalog data (typos, mis-entry).</summary>
public record CorrectBookDetailsCommand(
    int BookId, string? Title, string? Author,
    string? Isbn, int? Pages, int? TotalCopies) : IRequest<BookDto>;
