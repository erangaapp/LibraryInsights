using Inventory.Service.Application.Commands.CorrectBookDetails;
using Inventory.Service.Application.Exceptions;
using Inventory.Service.Domain.Entities;
using Inventory.Service.Tests.TestInfrastructure;

namespace Inventory.Service.Tests.Application;

public class CorrectBookDetailsHandlerTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public CorrectBookDetailsHandlerTests(SqliteDbFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync();
    }

    private static Book Seed(string isbn, string title = "Some Book") =>
        new(title, "Some Author", isbn, 200, new DateOnly(2024, 1, 1), 2);

    [Fact]
    public async Task CorrectsTitle_LeavesRestAlone()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var db = _fixture.CreateContext();
        db.Books.Add(Seed("978-111", title: "Teh Hobbit"));
        await db.SaveChangesAsync(ct);

        var handler = new CorrectBookDetailsCommandHandler(db);
        var result = await handler.Handle(
            new CorrectBookDetailsCommand(1, "The Hobbit", null, null, null, null),
            ct);

        Assert.Equal("The Hobbit", result.Title);
        Assert.Equal("978-111", result.Isbn);

        // persisted, not just returned
        using var check = _fixture.CreateContext();
        Assert.Equal("The Hobbit", (await check.Books.FindAsync([1, ct], 
            TestContext.Current.CancellationToken))!.Title);
    }

    [Fact]
    public async Task UnknownBook_ThrowsNotFound()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var handler = new CorrectBookDetailsCommandHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CorrectBookDetailsCommand(999, "x", null, null, null, null),
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task IsbnTakenByAnotherBook_ThrowsConflict()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var db = _fixture.CreateContext();
        db.Books.AddRange(Seed("978-111"), Seed("978-222"));
        await db.SaveChangesAsync(ct);

        var handler = new CorrectBookDetailsCommandHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CorrectBookDetailsCommand(2, null, null, "978-111", null, null),
                ct));
    }

    [Fact]
    public async Task ResendingOwnIsbn_IsNotAConflict()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var db = _fixture.CreateContext();
        db.Books.Add(Seed("978-111"));
        await db.SaveChangesAsync(ct);

        // Act
        var handler = new CorrectBookDetailsCommandHandler(db);
        var result = await handler.Handle(
            new CorrectBookDetailsCommand(1, null, null, "978-111", null, null),
            ct);

        Assert.Equal("978-111", result.Isbn);
    }
}
