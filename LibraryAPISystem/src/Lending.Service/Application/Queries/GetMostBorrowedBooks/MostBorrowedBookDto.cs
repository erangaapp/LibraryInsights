using MediatR;

namespace Lending.Service.Application.Queries.GetMostBorrowedBooks;

public record MostBorrowedBookDto(int BookId,
    string Title, string Author,
    int BorrowCount);
