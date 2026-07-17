namespace Lending.Service.Application.Queries.GetAlsoBorrowedBooks;

public record AlsoBorrowedBookDto(
    int BookId, string Title, 
    string Author, int SharedBorrowerCount);
