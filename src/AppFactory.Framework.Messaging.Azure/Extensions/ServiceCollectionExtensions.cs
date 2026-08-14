using Azure.Messaging.ServiceBus;
using AppFactory.Framework.Messaging.Azure.Configuration;
using AppFactory.Framework.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AppFactory.Framework.Messaging.Azure.Extensions;

/// <summary>
/// Dependency injection extensions for Azure messaging.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Azure Service Bus messaging services with the dependency injection container.
    /// </summary>
    public static IServiceCollection AddAzureServiceBus(
        this IServiceCollection services,
        Action<AzureServiceBusOptions> configureOptions)
    {
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        // Configure options
        services.Configure(configureOptions);

        // Register Service Bus client
        services.TryAddSingleton(provider =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureServiceBusOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new InvalidOperationException("ServiceBus ConnectionString is required");
            }

            return new ServiceBusClient(options.ConnectionString);
        });

        // Singleton: ServiceBusMessagePublisher holds a ServiceBusSender (AMQP link) that should
        // be reused across requests. All dependencies are singleton-safe.
        services.TryAddSingleton<IMessagePublisher, ServiceBusMessagePublisher>();

        return services;
    }

    /// <summary>
    /// Registers Azure Service Bus messaging services using configuration section.
    /// </summary>
    public static IServiceCollection AddAzureServiceBus(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        services.Configure<AzureServiceBusOptions>(configuration);

        // Register Service Bus client
        services.TryAddSingleton(provider =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureServiceBusOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new InvalidOperationException("ServiceBus ConnectionString is required");
            }

            return new ServiceBusClient(options.ConnectionString);
        });

        // Singleton: ServiceBusMessagePublisher holds a ServiceBusSender (AMQP link) that should
        // be reused across requests. All dependencies are singleton-safe.
        services.TryAddSingleton<IMessagePublisher, ServiceBusMessagePublisher>();

        return services;
    }
 
}