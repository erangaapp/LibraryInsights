using Lending.Service.Application.Queries.GetTopBorrowers;
using Lending.Service.Domain.Entities;

namespace Lending.Service.Tests.Application;

public class GetTopBorrowersHandlerTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public GetTopBorrowersHandlerTests(SqliteDbFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Borrower NewBorrower(string first, string last, string email) =>
        new() { FirstName = first, LastName = last, Email = email };

    private static Loan NewLoan(int bookId, int borrowerId, DateTime borrowedAt) =>
        new() { BookId = bookId, BorrowerId = borrowerId, BorrowedAt = borrowedAt };

    [Fact]
    public async Task RanksBorrowersByLoanCount_AndJoinsNames()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        db.Borrowers.AddRange(
            NewBorrower("Alice", "Silva", "alice@test.local"),   // id 1
            NewBorrower("Ben", "Fernando", "ben@test.local"),    // id 2
            NewBorrower("Cara", "Perera", "cara@test.local"));   // id 3
        await db.SaveChangesAsync(Ct);

        var now = DateTime.UtcNow;
        db.Loans.AddRange(
            NewLoan(1, 2, now), NewLoan(2, 2, now), NewLoan(3, 2, now), // Ben ×3
            NewLoan(1, 1, now), NewLoan(2, 1, now),                     // Alice ×2
            NewLoan(1, 3, now));                                        // Cara ×1
        await db.SaveChangesAsync(Ct);

        var handler = new GetTopBorrowersQueryHandler(db);

        // Act
        var result = await handler.Handle(
            new GetTopBorrowersQuery(null, null, Page: 1, PageSize: 10),
            Ct);


        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal([2, 1, 3], result.Items.Select(x => x.BorrowerId));
        Assert.Equal([3, 2, 1], result.Items.Select(x => x.BorrowCount));
        Assert.Equal("Ben", result.Items[0].FirstName);
        Assert.Equal("Fernando", result.Items[0].LastName);
    }

    [Fact]
    public async Task CountsOpenAndReturnedLoans()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        db.Borrowers.Add(NewBorrower("Alice", "Silva", "alice@test.local"));
        await db.SaveChangesAsync(Ct);

        var now = DateTime.UtcNow;
        var returned = NewLoan(1, 1, now.AddDays(-10));
        returned.ReturnedAt = now.AddDays(-5);
        db.Loans.AddRange(returned, NewLoan(2, 1, now)); // one returned, one open
        await db.SaveChangesAsync(Ct);

        var handler = new GetTopBorrowersQueryHandler(db);

        // Act
        var result = await handler.Handle(
            new GetTopBorrowersQuery(null, null, 1, 10), Ct);

        // Assert
        Assert.Equal(2, result.Items[0].BorrowCount); // "borrowed" = the act, not the return
    }

    [Fact]
    public async Task RespectsDateWindow_HalfOpenInterval()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        db.Borrowers.Add(NewBorrower("Alice", "Silva", "alice@test.local"));
        await db.SaveChangesAsync(Ct);

        var windowStart = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var windowEnd = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        db.Loans.AddRange(
            NewLoan(1, 1, windowStart.AddDays(-1)), // excluded
            NewLoan(1, 1, windowStart),             // included (>=)
            NewLoan(1, 1, windowStart.AddDays(10)), // included
            NewLoan(1, 1, windowEnd));              // excluded (<)
        await db.SaveChangesAsync(Ct);

        var handler = new GetTopBorrowersQueryHandler(db);

        // Act
        var result = await handler.Handle(
            new GetTopBorrowersQuery(windowStart, windowEnd, 1, 10),
            Ct);

        // Assert
        Assert.Equal(2, result.Items[0].BorrowCount);
    }
}
