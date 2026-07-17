using Lending.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lending.Service.Infrastructure.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans", t =>
            t.HasCheckConstraint("CK_Loans_Return_After_Borrow",
                "[ReturnedAt] IS NULL OR [ReturnedAt] >= [BorrowedAt]"));

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Borrower)
            .WithMany()
            .HasForeignKey(x => x.BorrowerId)
            .OnDelete(DeleteBehavior.Restrict);

        // BookId: intentionally no FK (cross-service reference) — plain indexed column
        builder.HasIndex(x => x.BookId);
        builder.HasIndex(x => new { x.BorrowerId, x.BorrowedAt });
        builder.HasIndex(x => x.BorrowedAt);

        #region Shadow Properties

        builder.Ignore(x => x.IsReturned);
        builder.Ignore(x => x.ReadingDays);

        #endregion
    }
}
