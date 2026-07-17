namespace Lending.Service.Application.Models.Responses;

/// <summary>Standard envelope for all paginated list responses.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
