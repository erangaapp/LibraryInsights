using InventoryDelegates = Inventory.Service.Endpoints.Delegates.InventoryEndpointDelegates;

namespace Inventory.Service.Endpoints;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this WebApplication app)
    {
        var inventory = app.MapGroup("/api/inventory");

        #region Books
        var books = inventory.MapGroup("/books");

        books.MapGet("", InventoryDelegates.GetBooksAsync);
        books.MapPost("", InventoryDelegates.CreateBookAsync);
        books.MapPost("/{bookId:int}/discontinue", InventoryDelegates.DiscontinueBookAsync);
        books.MapPatch("/{bookId:int}", InventoryDelegates.CorrectBookDetailsAsync);
        #endregion
    }
}
