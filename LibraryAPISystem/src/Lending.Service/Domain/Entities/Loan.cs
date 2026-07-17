namespace Lending.Service.Domain.Entities;

public class Loan
{
    public int Id { get; set; }

    /// <summary>Logical reference to a Book in the Inventory service.
    public int BookId { get; set; }

    public int BorrowerId { get; set; }
    public Borrower? Borrower { get; set; }

    public DateTime BorrowedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }

    public bool IsReturned => ReturnedAt.HasValue;

    /// <summary>Duration for reading-pace math. Same-day returns clamp
    /// to 1 day (continuous-reading assumption; avoids divide-by-zero).</summary>
    public int? ReadingDays => IsReturned
        ? Math.Max(1, (ReturnedAt!.Value.Date - BorrowedAt.Date).Days)
        : null;
}
