using Lending.Service.Application.Commands.BorrowBook;
using Lending.Service.Application.Exceptions;
using Lending.Service.Application.Models;
using Lending.Service.Domain.Entities;
using Lending.Service.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Tests.Application;

public class BorrowBookCommandHandlerTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public BorrowBookCommandHandlerTests(SqliteDbFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Borrower NewBorrower(int id, string first, string last) =>
        new() { Id = id, FirstName = first, LastName = last, 
            Email = $"{first.ToLower()}.{last.ToLower()}@test.local" };

    private static Loan NewActiveLoan(int bookId, int borrowerId) =>
        new() { BookId = bookId, BorrowerId = borrowerId, BorrowedAt = DateTime.UtcNow };

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenBorrowerDoesNotExist()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var catalog = new FakeBookCatalog(
            new CatalogBook(Id: 123, "The Hobbit", "Tolkien", 310, IsDiscontinued: false, Copies: 3)
        );
        var handler = new BorrowBookCommandHandler(db, catalog);
        var cmd = new BorrowBookCommand(BorrowerId: 999, BookId: 123);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(cmd, Ct));

        Assert.Contains("No borrower exists with id 999", ex.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenBookDoesNotExistInCatalog()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var borrower = NewBorrower(1, "Alice", "Silva");
        db.Borrowers.Add(borrower);
        await db.SaveChangesAsync(Ct);

        var catalog = new FakeBookCatalog(); // No books seeded
        var handler = new BorrowBookCommandHandler(db, catalog);
        var cmd = new BorrowBookCommand(456, borrower.Id);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(cmd, Ct));

        Assert.Contains("No book exists with id 456", ex.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenBookIsDiscontinued()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var borrower = NewBorrower(1, "Alice", "Silva");
        db.Borrowers.Add(borrower);
        await db.SaveChangesAsync(Ct);

        var catalog = new FakeBookCatalog(
            new CatalogBook(Id: 101, "Outdated Manual", "Unknown", 50, IsDiscontinued: true, Copies: 1)
        );
        var handler = new BorrowBookCommandHandler(db, catalog);
        var cmd = new BorrowBookCommand(BookId: 101, BorrowerId: borrower.Id);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(cmd, Ct));

        Assert.Contains("is discontinued and cannot be borrowed", ex.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenAllCopiesAreAlreadyOnLoan()
    {
        // Arrange
        using var db = _fixture.CreateContext();

        db.Borrowers.AddRange(
            NewBorrower(1, "Alice", "Silva"),
            NewBorrower(2, "Ben", "Fernando")
        );
        await db.SaveChangesAsync(Ct);

        // Only 1 copy is in circulation
        var catalog = new FakeBookCatalog(
            new CatalogBook(Id: 202, "Popular Book", "Author", 200, IsDiscontinued: false, Copies: 1)
        );

        // Alice currently has the only copy checked out
        db.Loans.Add(NewActiveLoan(bookId: 202, borrowerId: 1));
        await db.SaveChangesAsync(Ct);

        var handler = new BorrowBookCommandHandler(db, catalog);
        var cmd = new BorrowBookCommand(BorrowerId: 2, BookId: 202); // Ben tries to borrow it too

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(cmd, Ct));

        Assert.Contains("All 1 copies of book 202 are currently on loan", ex.Message);
    }

    [Fact]
    public async Task Handle_ShouldCreateLoanAndReturnDto_WhenEverythingIsValid()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var borrower = NewBorrower(1, "Alice", "Silva");
        db.Borrowers.Add(borrower);
        await db.SaveChangesAsync(Ct);

        var catalog = new FakeBookCatalog(
            new CatalogBook(Id: 303, "Clean Architecture", "Uncle Bob",
            400, IsDiscontinued: false, Copies: 5)
        );

        var handler = new BorrowBookCommandHandler(db, catalog);
        var cmd = new BorrowBookCommand(BookId: 303, BorrowerId: borrower.Id);

        // Act
        var result = await handler.Handle(cmd, Ct);

        // Assert DTO return properties
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(303, result.BookId);
        Assert.Equal(borrower.Id, result.BorrowerId);
        Assert.Null(result.ReturnedAt);
        Assert.True(result.BorrowedAt > DateTime.UtcNow.AddMinutes(-1));

        // Assert database persistence state
        var savedLoan = await db.Loans.
            FirstOrDefaultAsync(l => l.Id == result.Id, Ct);

        Assert.NotNull(savedLoan);
        Assert.Equal(303, savedLoan.BookId);
        Assert.Equal(borrower.Id, savedLoan.BorrowerId);
        Assert.Null(savedLoan.ReturnedAt);
    }
}
