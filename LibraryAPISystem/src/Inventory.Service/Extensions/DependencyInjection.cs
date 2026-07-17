using FluentValidation;
using Inventory.Service.Application.Behaviors;
using Inventory.Service.Handlers;
using Inventory.Service.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.RegisterValidations();
        services.RegisterHandlers();

        return services;
    }

    /// <summary>
    /// Registers the FluentValidation validators and the ValidationExceptionHandler
    /// </summary>
    /// <param name="services"></param>
    private static IServiceCollection RegisterValidations(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddProblemDetails();

        return services;
    }

    /// <summary>
    /// Registers the handlers for the application
    /// </summary>
    /// <param name="services"></param>
    private static IServiceCollection RegisterHandlers(this IServiceCollection services)
    {
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<CommandExceptionHandler>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(o =>
            o.UseSqlServer(configuration.GetConnectionString("InventoryDb")));

        return services;
    }
}
