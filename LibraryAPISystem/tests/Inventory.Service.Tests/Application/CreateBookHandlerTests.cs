using Inventory.Service.Application.Commands.CreateBook;
using Inventory.Service.Application.Exceptions;
using Inventory.Service.Domain.Entities;
using Inventory.Service.Tests.TestInfrastructure;

namespace Inventory.Service.Tests.Application;

public class CreateBookHandlerTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public CreateBookHandlerTests(SqliteDbFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CreatesBook_PersistsAndReturnsDto()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var handler = new CreateBookCommandHandler(db);

        // Act
        var result = await handler.Handle(
            new CreateBookCommand("The Name of the Wind", "Patrick Rothfuss",
                "9780756404741", 662, 3), Ct);

        Assert.True(result.Id > 0);
        Assert.Equal("The Name of the Wind", result.Title);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), result.DateReceived);

        // Act
        using var check = _fixture.CreateContext();
        var stored = await check.Books.
            FindAsync([result.Id, Ct], Ct);

        Assert.NotNull(stored);
        Assert.Equal("9780756404741", stored.Isbn);
        Assert.Null(stored.DiscontinuedDate); // new books are active
    }

    [Fact]
    public async Task DuplicateIsbn_ThrowsConflict()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        db.Books.Add(new Book("Existing", "Author", "9780756404741", 100,
            new DateOnly(2024, 1, 1)));
        await db.SaveChangesAsync(Ct);

        var handler = new CreateBookCommandHandler(db);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new CreateBookCommand("New Book", "Someone", "9780756404741", 200, 1),
                Ct));

        Assert.Contains("9780756404741", ex.Message);
    }

    [Fact]
    public async Task TrimsInputs_BeforePersisting()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var handler = new CreateBookCommandHandler(db);

        // Act
        var result = await handler.Handle(
            new CreateBookCommand("  Dune ", " Frank Herbert ", " 9780441172719 ", 412, 2),
            Ct);

        Assert.Equal("Dune", result.Title);
        Assert.Equal("Frank Herbert", result.Author);
        Assert.Equal("9780441172719", result.Isbn);
    }
}
