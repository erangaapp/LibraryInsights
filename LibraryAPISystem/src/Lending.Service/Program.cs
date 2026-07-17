
using Lending.Service.Endpoints;
using Lending.Service.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureHost();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.MigrateAndSeedAsync();

app.MapLendingEndpoints();
app.UseRequestLogging();

app.UseExceptionHandler();

app.Run();

public partial class Program { }