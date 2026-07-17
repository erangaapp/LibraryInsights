using Inventory.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Service.Infrastructure.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books", t =>
        {
            t.HasCheckConstraint("CK_Books_Pages_Positive",
                "[Pages] > 0");

            t.HasCheckConstraint("CK_Books_TotalCopies_NonNegative",
                "[TotalCopies] >= 0");

            t.HasCheckConstraint("CK_Books_Discontinued_After_Received", 
                "[DiscontinuedDate] IS NULL OR [DiscontinuedDate] >= [DateReceived]");
        });

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Isbn)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.Pages)
            .IsRequired();

        builder.Property(b => b.TotalCopies)
            .IsRequired();

        builder.Property(b => b.DateReceived)
            .IsRequired();

        builder.Property(b => b.DiscontinuedDate)
            .IsRequired(false);
    }
}
