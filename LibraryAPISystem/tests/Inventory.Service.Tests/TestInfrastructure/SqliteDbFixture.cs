using Inventory.Service.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Tests.TestInfrastructure;

public sealed class SqliteDbFixture : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public SqliteDbFixture()
    {
        _connection.Open();
        using var db = CreateContext();
        db.Database.EnsureCreated();
    }

    public InventoryDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(_connection).Options);

    public void ResetAsync()
    {
        using var db = CreateContext();
        db.Books.ExecuteDelete();
        db.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name = 'Books';");
    }


    public void Dispose() => _connection.Dispose();
}
