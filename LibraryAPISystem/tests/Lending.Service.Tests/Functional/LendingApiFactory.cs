using Lending.Service.Application.Abstractions;
using Lending.Service.Application.Models;
using Lending.Service.Infrastructure;
using Lending.Service.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lending.Service.Tests.Functional;

public class LendingApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseSetting("Database:MigrateAndSeedOnStartup", "false");

        builder.ConfigureTestServices(services =>
        {
            // Evict EVERYTHING EF registered for this context, by service type name
            var efServices = services
                .Where(d =>
                    d.ServiceType == typeof(LendingDbContext) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(DbContextOptions<LendingDbContext>) ||
                    d.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration") == true)
                .ToList();

            foreach (var d in efServices)
                services.Remove(d);

            services.AddDbContext<LendingDbContext>(o => o.UseSqlite(_connection));

            services.RemoveAll<IBookCatalog>();
            services.AddSingleton<IBookCatalog>(new FakeBookCatalog(
                new CatalogBook(1, "The Hobbit", "J.R.R. Tolkien", 310, false, 50)));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _connection.Dispose();
        base.Dispose(disposing);
    }
}
