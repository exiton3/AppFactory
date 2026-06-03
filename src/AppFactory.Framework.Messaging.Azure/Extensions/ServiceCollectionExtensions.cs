using Azure.Messaging.ServiceBus;
using Azure.Storage.Queues;
using AppFactory.Framework.Messaging.Azure.Configuration;
using AppFactory.Framework.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;

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

        // Register message publisher
        services.TryAddScoped<IMessagePublisher, ServiceBusMessagePublisher>();

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

        // Register message publisher
        services.TryAddScoped<IMessagePublisher, ServiceBusMessagePublisher>();

        return services;
    }

    /// <summary>
    /// Registers Azure Queue Storage messaging services with the dependency injection container.
    /// </summary>
    public static IServiceCollection AddAzureQueueStorage(
        this IServiceCollection services,
        Action<AzureQueueStorageOptions> configureOptions)
    {
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        // Configure options
        services.Configure(configureOptions);

        // Register Queue client
        services.TryAddSingleton(provider =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureQueueStorageOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new InvalidOperationException("Queue Storage ConnectionString is required");
            }

            if (string.IsNullOrWhiteSpace(options.QueueName))
            {
                throw new InvalidOperationException("Queue Storage QueueName is required");
            }

            var queueClient = new QueueClient(options.ConnectionString, options.QueueName);
            queueClient.CreateIfNotExists();
            return queueClient;
        });

        // Register message publisher
        services.TryAddScoped<IMessagePublisher, QueueStorageMessagePublisher>();

        return services;
    }

    /// <summary>
    /// Registers Azure Queue Storage messaging services using configuration section.
    /// </summary>
    public static IServiceCollection AddAzureQueueStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        services.Configure<AzureQueueStorageOptions>(configuration);

        // Register Queue client
        services.TryAddSingleton(provider =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureQueueStorageOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new InvalidOperationException("Queue Storage ConnectionString is required");
            }

            if (string.IsNullOrWhiteSpace(options.QueueName))
            {
                throw new InvalidOperationException("Queue Storage QueueName is required");
            }

            var queueClient = new QueueClient(options.ConnectionString, options.QueueName);
            queueClient.CreateIfNotExists();
            return queueClient;
        });

        // Register message publisher
        services.TryAddScoped<IMessagePublisher, QueueStorageMessagePublisher>();

        return services;
    }
}
