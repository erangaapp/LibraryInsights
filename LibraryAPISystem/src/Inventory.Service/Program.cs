using Inventory.Service.Endpoints;
using Inventory.Service.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureHost();

builder.Services.AddGrpc();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.MigrateAndSeedAsync();

app.MapGrpcEndpoints();
app.MapInventoryEndpoints();

app.UseRequestLogging();

app.UseExceptionHandler();

app.Run();

public partial class Program { }
