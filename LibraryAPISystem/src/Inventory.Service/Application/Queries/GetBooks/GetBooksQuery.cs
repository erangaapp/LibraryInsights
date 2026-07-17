using Inventory.Service.Application.Models.Responses;
using MediatR;

namespace Inventory.Service.Application.Queries.GetBooks;

public record GetBooksQuery(int Page = 1, int PageSize = 20,
    bool IncludeDiscontinued = false) : IRequest<PagedResult<BookDto>>;