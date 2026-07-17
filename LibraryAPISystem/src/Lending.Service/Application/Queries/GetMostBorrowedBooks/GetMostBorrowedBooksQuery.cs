using Lending.Service.Application.Models.Responses;
using MediatR;

namespace Lending.Service.Application.Queries.GetMostBorrowedBooks;

public record GetMostBorrowedBooksQuery(
    DateTime? From, DateTime? To, int Page = 1, int PageSize = 10)
    : IRequest<PagedResult<MostBorrowedBookDto>>;
