using Lending.Service.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Tests;

public sealed class SqliteDbFixture : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public SqliteDbFixture()
    {
        _connection.Open();
        using var db = CreateContext();
        db.Database.EnsureCreated();
    }

    public LendingDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<LendingDbContext>()
            .UseSqlite(_connection).Options);

    public void ResetAsync()
    {
        using var db = CreateContext();
        db.Loans.ExecuteDelete();
        db.Borrowers.ExecuteDelete();

        db.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name = 'Loans';");
        db.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name = 'Borrowers';");
    }

    public void Dispose() => _connection.Dispose();
}
