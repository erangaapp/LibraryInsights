using Lending.Service.Application.Models;
using Lending.Service.Application.Queries.GetReadingPace;
using Lending.Service.Domain.Entities;
using Lending.Service.Tests.TestInfrastructure;

namespace Lending.Service.Tests.Application;

public class GetReadingPaceQueryHandlerTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public GetReadingPaceQueryHandlerTests(SqliteDbFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Borrower NewBorrower(int id, string firstName, string lastName) =>
        new() { Id = id, FirstName = firstName, LastName = lastName, 
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@test.local" };

    private static Loan NewReturnedLoan(int bookId, int borrowerId, int readingDays) =>
        new()
        {
            BookId = bookId,
            BorrowerId = borrowerId,
            BorrowedAt = DateTime.UtcNow.AddDays(-30),
            ReturnedAt = DateTime.UtcNow.AddDays(-30 + readingDays)
        };

    [Fact]
    public async Task Handle_BorrowerDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var db = _fixture.CreateContext();

        // Empty catalog is fine since the handler exits early on the missing borrower
        var catalog = new FakeBookCatalog();
        var handler = new GetReadingPaceQueryHandler(db, catalog);

        // Act
        var result = await handler.Handle(new GetReadingPaceQuery(999), Ct);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_BorrowerHasNoReturnedLoans_ReturnsEmptyDto()
    {
        // Arrange
        using var db = _fixture.CreateContext();

        var borrower = NewBorrower(1, "Alice", "Silva");
        db.Borrowers.Add(borrower);

        var openLoan = new Loan { BookId = 10, BorrowerId = 1, BorrowedAt = DateTime.UtcNow };
        db.Loans.Add(openLoan);
        await db.SaveChangesAsync(Ct);

        var catalog = new FakeBookCatalog();
        var handler = new GetReadingPaceQueryHandler(db, catalog);

        // Act
        var result = await handler.Handle(new GetReadingPaceQuery(1), Ct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.BorrowerId);
        Assert.Equal("Alice Silva", result.BorrowerName);
        Assert.Null(result.AveragePagesPerDay);
        Assert.Empty(result.Books);
    }

    [Fact]
    public async Task Handle_WithReturnedLoans_CalculatesPacesAndSortsCorrectly()
    {
        // Arrange
        using var db = _fixture.CreateContext();

        var borrower = NewBorrower(2, "Ben", "Fernando");
        db.Borrowers.Add(borrower);

        // Book 101: 300 pages, read in 10 days = 30.0 pages/day
        // Book 102: 150 pages, read in 3 days = 50.0 pages/day (highest pace)
        db.Loans.AddRange(
            NewReturnedLoan(101, borrowerId: 2, readingDays: 10),
            NewReturnedLoan(102, borrowerId: 2, readingDays: 3)
        );
        await db.SaveChangesAsync(Ct);

        // Instantiate the fake catalog with predefined books
        var catalog = new FakeBookCatalog(
            new CatalogBook(101, "Book 1", "Some Author", 300, false),
            new CatalogBook(102, "Book 2", "Another Author", 150, false)
        );

        var handler = new GetReadingPaceQueryHandler(db, catalog);

        // Act
        var result = await handler.Handle(new GetReadingPaceQuery(2), Ct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Ben Fernando", result.BorrowerName);
        Assert.Equal(2, result.Books.Count);

        // Ordering check: 50.0 pages/day (Book 2) should come first
        var fastestBook = result.Books[0];
        Assert.Equal(102, fastestBook.BookId);
        Assert.Equal("Book 2", fastestBook.Title);
        Assert.Equal(50.0, fastestBook.PagesPerDay);

        var slowestBook = result.Books[1];
        Assert.Equal(101, slowestBook.BookId);
        Assert.Equal("Book 1", slowestBook.Title);
        Assert.Equal(30.0, slowestBook.PagesPerDay);

        // Average verification: (50.0 + 30.0) / 2 = 40.0
        Assert.Equal(40.0, result.AveragePagesPerDay);
    }
}
