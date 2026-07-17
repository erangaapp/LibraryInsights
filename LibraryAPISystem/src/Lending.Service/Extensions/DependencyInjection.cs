using FluentValidation;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Lending.Service.Application.Abstractions;
using Lending.Service.Application.Behaviors;
using Lending.Service.Handlers;
using Lending.Service.Infrastructure;
using Lending.Service.Infrastructure.Grpc;
using Library.Contracts.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Extensions;

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
        services.AddDbContext<LendingDbContext>(o =>
            o.UseSqlServer(configuration.GetConnectionString("LendingDb")));

        services.AddGrpcClient<InventoryGrpc.InventoryGrpcClient>(o => 
            o.Address = new Uri(configuration["Grpc:InventoryUrl"] 
            ?? throw new InvalidOperationException("Grpc:InventoryUrl not configured"))).
            ConfigureChannel(channel => channel.ServiceConfig = new ServiceConfig
            {
                MethodConfigs =
                {
                    new MethodConfig
                    {
                        Names = { MethodName.Default },
                        RetryPolicy = new RetryPolicy
                        {
                            MaxAttempts = 3,
                            InitialBackoff = TimeSpan.FromMilliseconds(200),
                            MaxBackoff = TimeSpan.FromSeconds(2),
                            BackoffMultiplier = 2,
                            RetryableStatusCodes = { StatusCode.Unavailable },
                        },
                    },
                },
            });

        services.RegisterAbstractions();

        return services;
    }

    /// <summary>
    /// Registers the abstractions for the Lending service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection RegisterAbstractions(this IServiceCollection services)
    {
        services.AddScoped<IBookCatalog, GrpcBookCatalog>();

        return services;
    }
}
