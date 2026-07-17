using Lending.Service.Application.Commands.CreateBorrower;
using Lending.Service.Application.Exceptions;
using Lending.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Tests.Application;

public class CreateBorrowerCommandHandlerTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public CreateBorrowerCommandHandlerTests(SqliteDbFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Borrower NewBorrower(string first, string last, string email) =>
        new() { FirstName = first, LastName = last, Email = email };

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        using var db = _fixture.CreateContext();

        // Seed an existing borrower with the target email
        db.Borrowers.Add(NewBorrower("Alice", "Silva", "alice@test.local"));
        await db.SaveChangesAsync(Ct);

        var handler = new CreateBorrowerCommandHandler(db);
        var cmd = new CreateBorrowerCommand("Bob", "Fernando", "alice@test.local");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(cmd, Ct));

        Assert.Contains("A borrower with email alice@test.local already exists", ex.Message);
    }

    [Fact]
    public async Task Handle_ShouldCreateBorrowerAndReturnDto_WhenEmailIsUnique()
    {
        // Arrange
        using var db = _fixture.CreateContext();

        var handler = new CreateBorrowerCommandHandler(db);
        var cmd = new CreateBorrowerCommand("Ben", "Fernando", "ben@test.local");

        // Act
        var result = await handler.Handle(cmd, Ct);

        // Assert DTO response properties
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Ben", result.FirstName);
        Assert.Equal("Fernando", result.LastName);
        Assert.Equal("ben@test.local", result.Email);

        // Assert database persistence state
        var savedBorrower = await db.Borrowers.
            FirstOrDefaultAsync(b => b.Id == result.Id, Ct);

        Assert.NotNull(savedBorrower);
        Assert.Equal("Ben", savedBorrower.FirstName);
        Assert.Equal("Fernando", savedBorrower.LastName);
        Assert.Equal("ben@test.local", savedBorrower.Email);
    }
}
