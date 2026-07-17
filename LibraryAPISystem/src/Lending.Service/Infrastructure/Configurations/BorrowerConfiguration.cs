using Lending.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lending.Service.Infrastructure.Configurations;

public class BorrowerConfiguration : IEntityTypeConfiguration<Borrower>
{
    public void Configure(EntityTypeBuilder<Borrower> builder)
    {
        builder.ToTable("Borrowers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.Email).HasMaxLength(200);

        builder.HasIndex(x => x.Email).IsUnique();

        #region Shadow Properties

        builder.Ignore(x => x.FullName);

        #endregion
    }
}
