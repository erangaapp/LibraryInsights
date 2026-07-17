using Lending.Service.Application.Models;
using Lending.Service.Application.Queries.GetAlsoBorrowedBooks;
using Lending.Service.Domain.Entities;
using Lending.Service.Tests.TestInfrastructure;

namespace Lending.Service.Tests.Application;

public class GetAlsoBorrowedBooksQueryHandlerTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public GetAlsoBorrowedBooksQueryHandlerTests(SqliteDbFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Borrower NewBorrower(int id, string firstName, string lastName) =>
        new() { Id = id, FirstName = firstName, LastName = lastName, 
            Email = $"{firstName.ToLower()}@test.local" };

    private static Loan NewLoan(int bookId, int borrowerId) =>
        new() { BookId = bookId, BorrowerId = borrowerId, BorrowedAt = DateTime.UtcNow };

    [Fact]
    public async Task Handle_ReturnsOtherBooksBorrowedBySamePeople_RankedByDistinctBorrowerCount()
    {
        // Arrange
        using var db = _fixture.CreateContext();

        // borrowers
        db.Borrowers.AddRange(
            NewBorrower(1, "Alice", "Silva"),
            NewBorrower(2, "Ben", "Fernando"),
            NewBorrower(3, "Cara", "Perera"),
            NewBorrower(4, "Dana", "Outsider"));
        await db.SaveChangesAsync(Ct);

        // Seed Book ID = 99
        // Alice (1), Ben (2), and Cara (3) all borrowed the seed book (99).
        // Alice and Ben also borrowed Book 101 (2 shared borrowers).
        // Only Cara borrowed Book 102 (1 shared borrower).
        // Ben also borrowed Book 103 (1 shared borrower).
        // Someone else (not in our cohort) borrowing Book 104 shouldn't affect the counts.
        db.Loans.AddRange(
            // Seed Book 99 loans
            NewLoan(99, borrowerId: 1),
            NewLoan(99, borrowerId: 2),
            NewLoan(99, borrowerId: 3),

            // Book 101 loans (2 shared borrowers: Alice and Ben)
            NewLoan(101, borrowerId: 1),
            NewLoan(101, borrowerId: 2),

            // Book 102 loans (1 shared borrower: Cara)
            NewLoan(102, borrowerId: 3),

            // Book 103 loans (1 shared borrower: Ben)
            NewLoan(103, borrowerId: 2),

            // Book 104 loan (0 shared borrowers - Borrower 4 did not borrow Seed Book 99)
            NewLoan(104, borrowerId: 4)
        );
        await db.SaveChangesAsync(Ct);

        // Set up the catalog fake with the metadata for our other books
        var catalog = new FakeBookCatalog(
            new CatalogBook(101, "Designing Data-Intensive Applications", "Martin Kleppmann", 400, false),
            new CatalogBook(102, "Clean Code", "Robert C. Martin", 464, false),
            new CatalogBook(103, "Refactoring", "Martin Fowler", 448, false)
        );

        var handler = new GetAlsoBorrowedBooksQueryHandler(db, catalog);

        // Act
        // Request the "also borrowed" books for Book ID 99
        var result = await handler.Handle(
            new GetAlsoBorrowedBooksQuery(BookId: 99, Page: 1, PageSize: 10),
            Ct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount); // Three distinct other books found (101, 102, 103)
        Assert.Equal(3, result.Items.Count);

        // First item should be Book 101 with 2 shared borrowers.
        var first = result.Items[0];
        Assert.Equal(101, first.BookId);
        Assert.Equal("Designing Data-Intensive Applications", first.Title);
        Assert.Equal("Martin Kleppmann", first.Author);
        Assert.Equal(2, first.SharedBorrowerCount);

        // Book 102 and 103 have 1 shared borrower each. 
        // Secondary ordering (ThenBy) is by BookId ascending, so 102 comes before 103.
        Assert.Equal(102, result.Items[1].BookId);
        Assert.Equal(103, result.Items[2].BookId);
    }

    [Fact]
    public async Task Handle_RespectsPagination()
    {
        // Arrange
        using var db = _fixture.CreateContext();

        db.Borrowers.Add(NewBorrower(1, "Alice", "Silva"));
        await db.SaveChangesAsync(Ct);

        db.Loans.AddRange(
            NewLoan(99, borrowerId: 1),  // Seed
            NewLoan(101, borrowerId: 1),
            NewLoan(102, borrowerId: 1),
            NewLoan(103, borrowerId: 1)
        );
        await db.SaveChangesAsync(Ct);

        var catalog = new FakeBookCatalog(
            new CatalogBook(101, "Book A", "Author A", 100, false),
            new CatalogBook(102, "Book B", "Author B", 100, false),
            new CatalogBook(103, "Book C", "Author C", 100, false)
        );

        var handler = new GetAlsoBorrowedBooksQueryHandler(db, catalog);

        // Act - Request Page 2 with a page size of 2
        var result = await handler.Handle(
            new GetAlsoBorrowedBooksQuery(BookId: 99, Page: 2, PageSize: 2),
            Ct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount); // 3 items total exist
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);

        // Page 2 should only have 1 item left (since Page 1 took the first 2)
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task Handle_WhenBookMetadataIsMissingFromCatalog_UsesUnknown()
    {
        // Arrange
        using var db = _fixture.CreateContext();

        db.Borrowers.Add(NewBorrower(1, "Alice", "Silva"));
        await db.SaveChangesAsync(Ct);

        db.Loans.AddRange(
            NewLoan(99, borrowerId: 1),
            NewLoan(101, borrowerId: 1) // Book 101 is borrowed, but missing from catalog
        );
        await db.SaveChangesAsync(Ct);

        // Fake catalog empty of Book 101 details
        var catalog = new FakeBookCatalog();

        var handler = new GetAlsoBorrowedBooksQueryHandler(db, catalog);

        // Act
        var result = await handler.Handle(
            new GetAlsoBorrowedBooksQuery(BookId: 99, Page: 1, PageSize: 10),
            Ct);

        // Assert
        Assert.Single(result.Items);

        // Verification of the CatalogBook.Unknown fallback values
        var fallbackBook = result.Items[0];
        Assert.Equal(101, fallbackBook.BookId);

        // Asserting fallback logic on missing metadata matches CatalogBook.Unknown expectations
        Assert.NotNull(fallbackBook.Title);
        Assert.Contains("Unknown", fallbackBook.Title);
    }
}
