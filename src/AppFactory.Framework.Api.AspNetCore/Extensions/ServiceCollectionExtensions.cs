using Microsoft.Extensions.DependencyInjection;
using AppFactory.Framework.Api.AspNetCore.Core;
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
        return services.AddAppFactoryApi(null, assemblies);
    }

    /// <summary>
    /// Add AppFactory API services for ASP.NET Core with options
    /// </summary>
    public static IServiceCollection AddAppFactoryApi(
        this IServiceCollection services,
        Action<AppFactoryApiOptions>? configureOptions,
        params System.Reflection.Assembly[] assemblies)
    {
        if (configureOptions != null)
            services.Configure(configureOptions);
        else
            services.AddOptions<AppFactoryApiOptions>();

        services.AddRequestParsing();
        services.AddScoped(typeof(IEndpointRequestHandler<,>), typeof(EndpointRequestHandler<,>));
        services.AddScoped(typeof(IEndpointResponseMapper<>), typeof(EndpointResponseMapper<>));

        if (assemblies?.Length > 0)
        {
            services.AddCqrs(assemblies);

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
