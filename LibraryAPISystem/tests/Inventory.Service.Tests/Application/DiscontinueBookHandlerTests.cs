using Inventory.Service.Application.Commands.DiscontinueBook;
using Inventory.Service.Application.Exceptions;
using Inventory.Service.Domain.Entities;
using Inventory.Service.Tests.TestInfrastructure;

namespace Inventory.Service.Tests.Application;

public class DiscontinueBookHandlerTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public DiscontinueBookHandlerTests(SqliteDbFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync();
    }

    [Fact]
    public async Task DiscontinuesActiveBook_SetsTodaysDate()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var db = _fixture.CreateContext();
        db.Books.Add(new Book("Old Edition", "Author", "978-001", 200,
            new DateOnly(2024, 1, 1)));
        await db.SaveChangesAsync(ct);

        //Act
        var handler = new DiscontinueBookCommandHandler(db);
        await handler.Handle(new DiscontinueBookCommand(1), ct);

        // Assert
        using var check = _fixture.CreateContext();
        var stored = await check.Books.FindAsync([1, ct], ct);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), stored!.DiscontinuedDate);
    }

    [Fact]
    public async Task AlreadyDiscontinued_ThrowsConflict()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var db = _fixture.CreateContext();
        var book = new Book("Old Edition", "Author", "978-001", 200,
            new DateOnly(2024, 1, 1));
        book.Discontinue(new DateOnly(2025, 1, 1));
        db.Books.Add(book);
        await db.SaveChangesAsync(ct);

        //Act
        var handler = new DiscontinueBookCommandHandler(db);

        // Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new DiscontinueBookCommand(1), ct));
    }

    [Fact]
    public async Task UnknownBook_ThrowsNotFound()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var db = _fixture.CreateContext();
        var handler = new DiscontinueBookCommandHandler(db);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new DiscontinueBookCommand(999), ct));
    }
}
