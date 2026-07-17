using Lending.Service.Application.Models.Responses;
using MediatR;

namespace Lending.Service.Application.Queries.GetAlsoBorrowedBooks;

public record GetAlsoBorrowedBooksQuery(int BookId, int Page = 1, int PageSize = 10)
    : IRequest<PagedResult<AlsoBorrowedBookDto>>;
