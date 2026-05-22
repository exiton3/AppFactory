using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Application.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AppFactory.Framework.Application;

/// <summary>
/// Extension methods for registering CQRS components in the dependency injection container
/// </summary>
public static class DependencyRegistrationExtensions
{
    /// <summary>
    /// Registers CQRS infrastructure including CommandDispatcher, Command Handlers, and Query Handlers
    /// from the specified assemblies
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for handlers. If none provided, scans the calling assembly</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCqrs(this IServiceCollection services, params Assembly[] assemblies)
    {
        var assembliesToScan = assemblies.Length > 0 
            ? assemblies 
            : new[] { Assembly.GetCallingAssembly() };

        // Register CommandDispatcher
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

        // Register all Command Handlers
        foreach (var assembly in assembliesToScan)
        {
            RegisterCommandHandlers(services, assembly);
        }

        // Register all Query Handlers
        foreach (var assembly in assembliesToScan)
        {
            RegisterQueryHandlers(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Registers only the CommandDispatcher and Command Handlers from the specified assemblies
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for command handlers. If none provided, scans the calling assembly</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCommandHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        var assembliesToScan = assemblies.Length > 0 
            ? assemblies 
            : new[] { Assembly.GetCallingAssembly() };

        // Register CommandDispatcher
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

        // Register all Command Handlers
        foreach (var assembly in assembliesToScan)
        {
            RegisterCommandHandlers(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Registers only Query Handlers from the specified assemblies
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for query handlers. If none provided, scans the calling assembly</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddQueryHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        var assembliesToScan = assemblies.Length > 0 
            ? assemblies 
            : new[] { Assembly.GetCallingAssembly() };

        // Register all Query Handlers
        foreach (var assembly in assembliesToScan)
        {
            RegisterQueryHandlers(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Registers a specific Command Handler
    /// </summary>
    /// <typeparam name="TCommandHandler">The command handler implementation type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCommandHandler<TCommandHandler>(this IServiceCollection services) 
        where TCommandHandler : class, ICommandHandler
    {
        var handlerType = typeof(TCommandHandler);
        
        // Register as ICommandHandler (non-generic) for CommandDispatcher
        services.AddScoped<ICommandHandler, TCommandHandler>();
        
        // Register as ICommandHandler<TCommand> for direct injection
        var genericInterface = handlerType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>));
        
        if (genericInterface != null)
        {
            services.AddScoped(genericInterface, handlerType);
        }

        return services;
    }

    /// <summary>
    /// Registers a specific Query Handler
    /// </summary>
    /// <typeparam name="TQueryHandler">The query handler implementation type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddQueryHandler<TQueryHandler>(this IServiceCollection services) 
        where TQueryHandler : class
    {
        var handlerType = typeof(TQueryHandler);
        
        // Register as IQueryHandler<TRequest, TResponse> for direct injection
        var queryHandlerInterface = handlerType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
        
        if (queryHandlerInterface != null)
        {
            services.AddScoped(queryHandlerInterface, handlerType);
        }

        return services;
    }

    #region Private Helper Methods

    private static void RegisterCommandHandlers(IServiceCollection services, Assembly assembly)
    {
        var commandHandlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ICommandHandler).IsAssignableFrom(t))
            .ToList();

        foreach (var handlerType in commandHandlerTypes)
        {
            // Register as ICommandHandler (non-generic) for CommandDispatcher
            services.AddScoped(typeof(ICommandHandler), handlerType);

            // Register as ICommandHandler<TCommand> for direct injection
            var genericInterface = handlerType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>));

            if (genericInterface != null)
            {
                services.AddScoped(genericInterface, handlerType);
            }
        }
    }

    private static void RegisterQueryHandlers(IServiceCollection services, Assembly assembly)
    {
        var queryHandlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
            .ToList();

        foreach (var handlerType in queryHandlerTypes)
        {
            var queryHandlerInterfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));

            foreach (var interfaceType in queryHandlerInterfaces)
            {
                services.AddScoped(interfaceType, handlerType);
            }
        }
    }

    #endregion
}
