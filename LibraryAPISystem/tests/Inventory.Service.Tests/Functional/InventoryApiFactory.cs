using Inventory.Service.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Service.Tests.Functional;

public class InventoryApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseSetting("Database:MigrateAndSeedOnStartup", "false");

        builder.ConfigureTestServices(services =>
        {
            var efServices = services
                .Where(d =>
                    d.ServiceType == typeof(InventoryDbContext) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(DbContextOptions<InventoryDbContext>) ||
                    d.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration") == true)
                .ToList();

            foreach (var d in efServices)
                services.Remove(d);

            services.AddDbContext<InventoryDbContext>(o => o.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _connection.Dispose();
        base.Dispose(disposing);
    }
}
