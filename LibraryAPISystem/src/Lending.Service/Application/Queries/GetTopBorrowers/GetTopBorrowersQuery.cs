using Lending.Service.Application.Models.Responses;
using MediatR;

namespace Lending.Service.Application.Queries.GetTopBorrowers;

public record GetTopBorrowersQuery(
    DateTime? From, DateTime? To, int Page = 1, int PageSize = 10)
    : IRequest<PagedResult<TopBorrowerDto>>;
