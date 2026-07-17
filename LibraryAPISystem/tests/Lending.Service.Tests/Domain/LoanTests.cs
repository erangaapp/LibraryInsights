using Lending.Service.Domain.Entities;

namespace Lending.Service.Tests.Domain;

public class LoanTests
{
    [Fact]
    public void ReadingDays_IsNull_WhileLoanIsOpen()
    {
        var loan = new Loan { BorrowedAt = DateTime.UtcNow };

        Assert.False(loan.IsReturned);
        Assert.Null(loan.ReadingDays);
    }

    [Fact]
    public void ReadingDays_ClampsSameDayReturn_ToOneDay()
    {
        var borrowed = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc);
        var loan = new Loan { BorrowedAt = borrowed, ReturnedAt = borrowed.AddHours(5) };

        Assert.Equal(1, loan.ReadingDays);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(28)]
    public void ReadingDays_ComputesCalendarDays(int days)
    {
        var borrowed = new DateTime(2026, 6, 1, 14, 0, 0, DateTimeKind.Utc);
        var loan = new Loan { BorrowedAt = borrowed, ReturnedAt = borrowed.AddDays(days) };

        Assert.Equal(days, loan.ReadingDays);
    }
}
