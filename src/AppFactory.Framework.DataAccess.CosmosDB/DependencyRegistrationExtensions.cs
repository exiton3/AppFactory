using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using AppFactory.Framework.DataAccess.CosmosDB.Settings;
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
    /// Registers all Model Configs from the specified assemblies
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for model configs. If none provided, scans the calling assembly</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterModelConfigs(this IServiceCollection services, params Assembly[] assemblies)
    {
        var assembliesToScan = assemblies.Length > 0 
            ? assemblies 
            : new[] { Assembly.GetCallingAssembly() };

        foreach (var assembly in assembliesToScan)
        {
            RegisterModelConfigsFromAssembly(services, assembly);
        }

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
    /// Registers all Repositories from the specified assemblies
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for repositories. If none provided, scans the calling assembly</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterRepositories(this IServiceCollection services, params Assembly[] assemblies)
    {
        var assembliesToScan = assemblies.Length > 0 
            ? assemblies 
            : new[] { Assembly.GetCallingAssembly() };

        foreach (var assembly in assembliesToScan)
        {
            RegisterRepositoriesFromAssembly(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Registers all Model Configs and Repositories from the specified assemblies
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

        // Register all Model Configs and Repositories
        foreach (var assembly in assembliesToScan)
        {
            RegisterModelConfigsFromAssembly(services, assembly);
            RegisterRepositoriesFromAssembly(services, assembly);
        }

        return services;
    }

    #region Private Helper Methods

    private static void RegisterModelConfigsFromAssembly(IServiceCollection services, Assembly assembly)
    {
        var modelConfigTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IModelConfig<>)))
            .ToList();

        foreach (var configType in modelConfigTypes)
        {
            var modelConfigInterface = configType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IModelConfig<>));

            if (modelConfigInterface != null)
            {
                services.AddSingleton(modelConfigInterface, configType);
            }
        }
    }

    private static void RegisterRepositoriesFromAssembly(IServiceCollection services, Assembly assembly)
    {
        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<>)))
            .ToList();

        foreach (var repositoryType in repositoryTypes)
        {
            var repositoryInterfaces = repositoryType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<>));

            foreach (var interfaceType in repositoryInterfaces)
            {
                services.AddScoped(interfaceType, repositoryType);
            }
        }
    }

    #endregion
}
