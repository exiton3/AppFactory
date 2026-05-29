using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AppFactory.Framework.Messaging.Abstractions;

namespace AppFactory.Framework.Messaging.Extensions;

/// <summary>
/// Service collection extensions for messaging
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register a message handler
    /// </summary>
    public static IServiceCollection AddMessageHandler<THandler, TMessage>(this IServiceCollection services)
        where THandler : class, IMessageHandler<TMessage>
        where TMessage : class
    {
        services.TryAddScoped<IMessageHandler<TMessage>, THandler>();
        return services;
    }

    /// <summary>
    /// Register message handlers from assembly
    /// </summary>
    public static IServiceCollection AddMessageHandlers(this IServiceCollection services, params System.Reflection.Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>)))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                var interfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>));

                foreach (var @interface in interfaces)
                {
                    services.TryAddScoped(@interface, handlerType);
                }
            }
        }

        return services;
    }
}
