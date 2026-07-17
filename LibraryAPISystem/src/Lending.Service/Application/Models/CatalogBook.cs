namespace Lending.Service.Application.Models;

public record CatalogBook(int Id, 
    string Title, string Author, 
    int Pages, bool IsDiscontinued = false, int? Copies = null)
{
    public const string UnknownTitle = "Unknown title";

    /// <summary>
    /// Placeholder for book ids absent from the Inventory catalog
    /// </summary>
    public static CatalogBook Unknown(int id) => new(id, UnknownTitle, string.Empty, 0);
}
