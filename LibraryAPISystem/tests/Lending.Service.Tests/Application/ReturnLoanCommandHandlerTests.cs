using Lending.Service.Application.Commands.ReturnLoan;
using Lending.Service.Application.Exceptions;
using Lending.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Tests.Application;

public class ReturnLoanCommandHandlerTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public ReturnLoanCommandHandlerTests(SqliteDbFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Loan NewActiveLoan(int id, int bookId, int borrowerId) =>
        new()
        {
            Id = id,
            BookId = bookId,
            BorrowerId = borrowerId,
            BorrowedAt = DateTime.UtcNow.AddDays(-7)
        };

    private static Loan NewReturnedLoan(int id, int bookId, int borrowerId, DateTime returnedAt) =>
        new()
        {
            Id = id,
            BookId = bookId,
            BorrowerId = borrowerId,
            BorrowedAt = DateTime.UtcNow.AddDays(-14),
            ReturnedAt = returnedAt
        };

    private static Borrower NewBorrower(int id, string firstName = "Test", string lastName = "User") => 
        new()
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}{id}@test.local",
        };

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenLoanDoesNotExist()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var handler = new ReturnLoanCommandHandler(db);
        var cmd = new ReturnLoanCommand(LoanId: 999);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(cmd, Ct));

        Assert.Contains("No loan exists with id 999", ex.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenLoanIsAlreadyReturned()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var alreadyReturnedAt = new DateTime(2026, 7, 10, 12, 0, 0, DateTimeKind.Utc);

        db.Borrowers.Add(NewBorrower(1, "Alice", "Silva"));
        await db.SaveChangesAsync(Ct);

        // Seed an already returned loan
        db.Loans.Add(NewReturnedLoan(id: 42, bookId: 101, borrowerId: 1, alreadyReturnedAt));
        await db.SaveChangesAsync(Ct);

        var handler = new ReturnLoanCommandHandler(db);
        var cmd = new ReturnLoanCommand(LoanId: 42);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(cmd, Ct));

        Assert.Contains("already returned on 2026-07-10 12:00:00Z", ex.Message);
    }

    [Fact]
    public async Task Handle_ShouldMarkAsReturnedAndSave_WhenLoanIsActive()
    {
        // Arrange
        using var db = _fixture.CreateContext();

        db.Borrowers.Add(NewBorrower(1, "Alice", "Silva"));
        await db.SaveChangesAsync(Ct);

        // Seed an active loan (ReturnedAt is null)
        db.Loans.Add(NewActiveLoan(id: 10, bookId: 101, borrowerId: 1));
        await db.SaveChangesAsync(Ct);

        var handler = new ReturnLoanCommandHandler(db);
        var cmd = new ReturnLoanCommand(LoanId: 10);

        // Act
        var result = await handler.Handle(cmd, Ct);

        // Assert DTO response properties
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.NotNull(result.ReturnedAt);
        Assert.True(result.ReturnedAt > DateTime.UtcNow.AddMinutes(-1)); // check on the newly set UTC timestamp

        // Assert database persistence state
        var savedLoan = await db.Loans.
            FirstOrDefaultAsync(l => l.Id == 10, Ct);
        Assert.NotNull(savedLoan);
        Assert.NotNull(savedLoan.ReturnedAt);
        Assert.Equal(result.ReturnedAt, savedLoan.ReturnedAt);
    }
}
