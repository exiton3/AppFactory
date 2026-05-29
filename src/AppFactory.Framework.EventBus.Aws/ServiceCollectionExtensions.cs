using Amazon.EventBridge;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.EventBus.Abstractions;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.EventBus.Aws;

/// <summary>
/// Dependency injection extensions for AWS EventBridge
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register AWS EventBridge publisher
    /// </summary>
    public static IServiceCollection AddEventBridgePublisher(
        this IServiceCollection services,
        string eventBusName = "default")
    {
        services.AddSingleton<IAmazonEventBridge>(sp => new AmazonEventBridgeClient());

        services.AddSingleton<IEventPublisher>(sp =>
        {
            var eventBridge = sp.GetRequiredService<IAmazonEventBridge>();
            var serializer = sp.GetRequiredService<IJsonSerializer>();
            var logger = sp.GetRequiredService<Logging.ILogger>();

            return new EventBridgePublisher(eventBridge, serializer, logger, eventBusName);
        });

        return services;
    }

    /// <summary>
    /// Register event handlers from assembly
    /// </summary>
    public static IServiceCollection AddEventHandlers(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IEventHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
