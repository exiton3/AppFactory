using Microsoft.Extensions.DependencyInjection;
using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Application;
using AppFactory.Framework.DependencyInjection;

namespace AppFactory.Framework.Api.AspNetCore.Extensions;

/// <summary>
/// Extension methods for configuring AppFactory services in ASP.NET Core
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add AppFactory API services for ASP.NET Core
    /// </summary>
    public static IServiceCollection AddAppFactoryApi(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        services.AddRequestParsing();

        // Add CQRS and Processors if assemblies provided
        if (assemblies?.Length > 0)
        {
            services.AddCqrs(assemblies);

            // Register all IFunctionProcessor implementations
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IFunctionProcessor<,>)), publicOnly: false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());
        }

        return services;
    }

    /// <summary>
    /// Add AppFactory API services with CQRS from specified assemblies
    /// </summary>
    public static IServiceCollection AddAppFactoryApiWithCqrs(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        return services.AddAppFactoryApi(assemblies);
    }
}
