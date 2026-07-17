using Lending.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Infrastructure;

public class LendingDbContext(DbContextOptions<LendingDbContext> options) : 
    DbContext(options)
{
    #region DbSets
    public DbSet<Borrower> Borrowers => Set<Borrower>();
    public DbSet<Loan> Loans => Set<Loan>();
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(LendingDbContext).Assembly);
}
