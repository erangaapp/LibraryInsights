namespace Inventory.Service.Application.Queries.GetBooks;

public record BookDto(int Id, string Title, string Author, string Isbn,
    int Pages, int TotalCopies, DateOnly DateReceived, bool IsDiscontinued = false);
