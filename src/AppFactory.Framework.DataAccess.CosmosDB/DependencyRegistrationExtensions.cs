using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using AppFactory.Framework.DataAccess.CosmosDB.Settings;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AppFactory.Framework.DataAccess.CosmosDB;

/// <summary>
/// Extension methods for registering CosmosDB persistence components in the dependency injection container
/// </summary>
public static class DependencyRegistrationExtensions
{
    /// <summary>
    /// Registers the CosmosDB infrastructure including client factory and settings
    /// </summary>
    /// <param name="services">The service collection</param>
    public static void RegisterCosmosDbPersistence(this IServiceCollection services)
    {
        services.AddSingleton<ICosmosDbClientFactory, CosmosDbClientFactory>();
        services.AddScoped<ICosmosDbSettings, CosmosDbSettings>();
    }

    /// <summary>
    /// Registers a specific Model Config for a given model type
    /// </summary>
    /// <typeparam name="TModelConfig">The model config implementation type</typeparam>
    /// <typeparam name="TModel">The model type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterModelConfig<TModelConfig, TModel>(this IServiceCollection services) 
        where TModelConfig : IModelConfig<TModel> 
        where TModel : class
    {
        services.AddSingleton(typeof(IModelConfig<TModel>), typeof(TModelConfig));
        return services;
    }

    /// <summary>
    /// Registers all Model Configs from the specified assemblies using assembly scanning
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for model configs. If none provided, scans the calling assembly</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterModelConfigs(this IServiceCollection services, params Assembly[] assemblies)
    {
        var assembliesToScan = assemblies.Length > 0 
            ? assemblies 
            : new[] { Assembly.GetCallingAssembly() };

        services.Scan(scan => scan
            .FromAssemblies(assembliesToScan)
            .AddClasses(classes => classes.AssignableTo(typeof(IModelConfig<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

        return services;
    }

    /// <summary>
    /// Registers a specific Repository for a given model type
    /// </summary>
    /// <typeparam name="TRepository">The repository implementation type</typeparam>
    /// <typeparam name="TModel">The model type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterRepository<TRepository, TModel>(this IServiceCollection services) 
        where TRepository : class, IRepository<TModel>
        where TModel : class
    {
        services.AddScoped<IRepository<TModel>, TRepository>();
        return services;
    }

    /// <summary>
    /// Registers all Repositories from the specified assemblies using assembly scanning
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for repositories. If none provided, scans the calling assembly</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterRepositories(this IServiceCollection services, params Assembly[] assemblies)
    {
        var assembliesToScan = assemblies.Length > 0 
            ? assemblies 
            : new[] { Assembly.GetCallingAssembly() };

        services.Scan(scan => scan
            .FromAssemblies(assembliesToScan)
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Registers all Model Configs and Repositories from the specified assemblies using assembly scanning
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for model configs and repositories. If none provided, scans the calling assembly</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterCosmosDbDataAccess(this IServiceCollection services, params Assembly[] assemblies)
    {
        var assembliesToScan = assemblies.Length > 0 
            ? assemblies 
            : new[] { Assembly.GetCallingAssembly() };

        // Register infrastructure
        RegisterCosmosDbPersistence(services);

        // Use assembly scanning to register all Model Configs and Repositories
        services.Scan(scan => scan
            .FromAssemblies(assembliesToScan)
            .AddClasses(classes => classes.AssignableTo(typeof(IModelConfig<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
