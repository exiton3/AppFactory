using Amazon;
using Amazon.SQS;
using AppFactory.Framework.Messaging.Aws.Configuration;
using AppFactory.Framework.Messaging.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AppFactory.Framework.Messaging.Aws.Extensions;

/// <summary>
/// Dependency injection extensions for AWS SQS messaging.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers AWS SQS messaging services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Configuration action for SQS options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAwsMessaging(
        this IServiceCollection services,
        Action<AwsSqsOptions> configureOptions)
    {
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        // Configure options
        services.Configure(configureOptions);

        // Register AWS SQS client
        services.TryAddSingleton<IAmazonSQS>(provider =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AwsSqsOptions>>().Value;

            var config = new AmazonSQSConfig();

            if (!string.IsNullOrWhiteSpace(options.Region))
            {
                config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
            }

            return new AmazonSQSClient(config);
        });

        // Register message publisher
        services.TryAddScoped<IMessagePublisher, SqsMessagePublisher>();

        return services;
    }

    /// <summary>
    /// Registers AWS SQS messaging services using configuration section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section containing SQS options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAwsMessaging(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        services.Configure<AwsSqsOptions>(configuration);

        // Register AWS SQS client
        services.TryAddSingleton<IAmazonSQS>(provider =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AwsSqsOptions>>().Value;

            var config = new AmazonSQSConfig();

            if (!string.IsNullOrWhiteSpace(options.Region))
            {
                config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
            }

            return new AmazonSQSClient(config);
        });

        // Register message publisher
        services.TryAddScoped<IMessagePublisher, SqsMessagePublisher>();

        return services;
    }
}
