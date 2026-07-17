using Inventory.Service.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Inventory.Service.Tests.Functional;

public class InventoryApiTests : IClassFixture<InventoryApiFactory>
{
    private readonly InventoryApiFactory _factory;
    private readonly HttpClient _client;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public InventoryApiTests(InventoryApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<InventoryDbContext>()
            .Database.EnsureCreated();
    }

    [Fact]
    public async Task Catalog_ReturnsPagedEnvelope()
    {
        var response = await _client.GetAsync("/api/inventory/books?page=1&pageSize=5", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PagedResultDto>(Ct);
        Assert.Equal(1, body!.Page);
    }

    [Fact]
    public async Task CreateBook_Then_DuplicateIsbn_Returns201Then409()
    {
        var isbn = $"978{Guid.NewGuid():N}"[..13]; // unique per run, fits max length

        var payload = new
        {
            title = "Functional Test Book",
            author = "Test Author",
            isbn,
            pages = 250,
            totalCopies = 2,
        };

        var created = await _client.PostAsJsonAsync("/api/inventory/books", payload, Ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);

        var duplicate = await _client.PostAsJsonAsync("/api/inventory/books", payload, Ct);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task CorrectBook_WithNoFields_Returns400()
    {
        // create a book to correct — self-contained
        var created = await _client.PostAsJsonAsync("/api/inventory/books", new
        {
            title = "To Correct",
            author = "A",
            isbn = $"979{Guid.NewGuid():N}"[..13],
            pages = 100,
            totalCopies = 1,
        }, Ct);
        var book = await created.Content.ReadFromJsonAsync<BookDtoLite>(Ct);

        var response = await _client.PatchAsJsonAsync(
            $"/api/inventory/books/{book!.Id}", new { }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private record PagedResultDto(int Page, int PageSize, int TotalCount);
    private record BookDtoLite(int Id);
}
