namespace Inventory.Service.Application.Queries.GetBooksByIds;

public record BookEnrichmentDto(int Id,
    string Title,
    string Author,
    int Pages);
