using Inventory.Service.Infrastructure;
using Inventory.Service.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Inventory.Service.Extensions;

public static class WebExtensions
{
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("Database:MigrateAndSeedOnStartup"))
            return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await db.Database.MigrateAsync();
        await InventoryDataSeeder.SeedAsync(db);
    }

    public static WebApplication UseRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(o =>
        {
            o.EnrichDiagnosticContext = (diag, http) =>
            {
                diag.Set("TraceId", System.Diagnostics.Activity.Current?.TraceId.ToString());
                diag.Set("RequestPath", http.Request.Path);
            };
        });

        return app;
    }
}
