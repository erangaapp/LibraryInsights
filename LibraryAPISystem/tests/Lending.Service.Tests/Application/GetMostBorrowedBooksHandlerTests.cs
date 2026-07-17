using Lending.Service.Application.Models;
using Lending.Service.Application.Queries.GetMostBorrowedBooks;
using Lending.Service.Domain.Entities;
using Lending.Service.Tests.TestInfrastructure;

namespace Lending.Service.Tests.Application;

public class GetMostBorrowedBooksHandlerTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public GetMostBorrowedBooksHandlerTests(SqliteDbFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Borrower NewBorrower(int i) =>
        new() { FirstName = $"FN {i}", LastName = $"LN {i}", Email = $"b{i}@test.local" };

    private static Loan NewLoan(int bookId, int borrowerId, DateTime borrowedAt) =>
        new() { BookId = bookId, BorrowerId = borrowerId, BorrowedAt = borrowedAt };


    [Fact]
    public async Task RanksBooksByBorrowCount_AndEnrichesTitles()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        db.Borrowers.AddRange(NewBorrower(1), NewBorrower(2), NewBorrower(3));
        await db.SaveChangesAsync(Ct);

        var now = DateTime.UtcNow;
        db.Loans.AddRange(
            NewLoan(1, 1, now), NewLoan(1, 2, now), NewLoan(1, 3, now), // book 1 ×3
            NewLoan(2, 1, now), NewLoan(2, 2, now),                     // book 2 ×2
            NewLoan(3, 1, now));                                        // book 3 ×1
        await db.SaveChangesAsync(Ct);

        var catalog = new FakeBookCatalog(
            new CatalogBook(1, "The Hobbit", "Tolkien", 310, false),
            new CatalogBook(2, "1984", "Orwell", 328, false),
            new CatalogBook(3, "Dune", "Herbert", 412, false));

        var handler = new GetMostBorrowedBooksQueryHandler(db, catalog);

        // Act
        var result = await handler.Handle(
            new GetMostBorrowedBooksQuery(null, null, Page: 1, PageSize: 10),
            Ct);

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal([1, 2, 3], result.Items.Select(x => x.BookId));
        Assert.Equal([3, 2, 1], result.Items.Select(x => x.BorrowCount));
        Assert.Equal("The Hobbit", result.Items[0].Title);
    }

    [Fact]
    public async Task RespectsDateWindow_HalfOpenInterval()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        db.Borrowers.Add(NewBorrower(1));
        await db.SaveChangesAsync(Ct);

        var windowStart = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var windowEnd = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        db.Loans.AddRange(
            NewLoan(1, 1, windowStart.AddDays(-1)),  // before -> excluded
            NewLoan(1, 1, windowStart),              // boundary -> included (>=)
            NewLoan(1, 1, windowStart.AddDays(10)),  // inside -> included
            NewLoan(1, 1, windowEnd));               // end boundary -> EXCLUDED (<)
        await db.SaveChangesAsync(Ct);

        var handler = new GetMostBorrowedBooksQueryHandler(
            db, new FakeBookCatalog(new CatalogBook(1, "The Hobbit", "Tolkien", 310, false)));

        // Act
        var result = await handler.Handle(
            new GetMostBorrowedBooksQuery(windowStart, windowEnd, 1, 10), Ct);

        // Assert
        Assert.Equal(2, result.Items[0].BorrowCount);
    }

    [Fact]
    public async Task RendersUnknownTitle_WhenBookMissingFromCatalog()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        db.Borrowers.Add(NewBorrower(1));
        await db.SaveChangesAsync(Ct);
        db.Loans.Add(NewLoan(31, 1, DateTime.UtcNow));
        await db.SaveChangesAsync(Ct);

        var handler = new GetMostBorrowedBooksQueryHandler(db, new FakeBookCatalog());

        // Act
        var result = await handler.Handle(
            new GetMostBorrowedBooksQuery(null, null, 1, 10), Ct);

        // Assert
        Assert.Equal(CatalogBook.UnknownTitle, result.Items[0].Title);
        Assert.Equal(1, result.Items[0].BorrowCount); // count survives missing catalog
    }
}
