using Inventory.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Infrastructure;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{

    #region DbSets
    public DbSet<Book> Books => Set<Book>();
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
}
