using MediatR;

namespace Inventory.Service.Application.Queries.GetBooksByIds;

public record GetBooksByIdsQuery(IReadOnlyList<int> Ids)
    : IRequest<IReadOnlyList<BookEnrichmentDto>>;
