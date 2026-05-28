using Microsoft.Extensions.DependencyInjection;
using AppFactory.Framework.Api.Parsing;
using AppFactory.Framework.Application;
using AppFactory.Framework.Shared.Serialization;

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
        // Add core services
        services.AddSingleton<IJsonSerializer, DefaultJsonSerializer>();
        services.AddSingleton<IRequestParser, RequestParser>();

        // Add CQRS if assemblies provided
        if (assemblies?.Length > 0)
        {
            services.AddCqrs(assemblies);
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
