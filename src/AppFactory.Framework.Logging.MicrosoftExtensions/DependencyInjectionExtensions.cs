using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppFactory.Framework.Logging.MicrosoftExtensions;

/// <summary>
/// Extension methods for registering Microsoft.Extensions.Logging-based logging in the dependency injection container
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers AppFactory logging using Microsoft.Extensions.Logging as the provider.
    /// Assumes Microsoft.Extensions.Logging is already configured in the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMicrosoftExtensionsLogging(this IServiceCollection services)
    {
        // Register factory that bridges AppFactory.ILoggerFactory to Microsoft.Extensions.Logging.ILoggerFactory
        services.AddSingleton<ILoggerFactory, MicrosoftExtensionsLoggerFactory>();

        // Register ILogger as singleton (loggers are typically stateless and thread-safe)
        services.AddSingleton<ILogger>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
            var melLogger = loggerFactory.CreateLogger("AppFactory");
            return new MicrosoftExtensionsLogger(melLogger);
        });

        return services;
    }

    /// <summary>
    /// Registers AppFactory logging using Microsoft.Extensions.Logging with a specific category name
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="categoryName">The category name for the logger</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMicrosoftExtensionsLogging(
        this IServiceCollection services, 
        string categoryName)
    {
        services.AddSingleton<ILoggerFactory, MicrosoftExtensionsLoggerFactory>();

        services.AddSingleton<ILogger>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
            var melLogger = loggerFactory.CreateLogger(categoryName);
            return new MicrosoftExtensionsLogger(melLogger);
        });

        return services;
    }

    /// <summary>
    /// Registers AppFactory logging using Microsoft.Extensions.Logging with configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Action to configure Microsoft.Extensions.Logging</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMicrosoftExtensionsLogging(
        this IServiceCollection services,
        Action<ILoggingBuilder> configure)
    {
        // Add and configure Microsoft.Extensions.Logging
        services.AddLogging(configure);

        // Register AppFactory adapters
        services.AddSingleton<ILoggerFactory, MicrosoftExtensionsLoggerFactory>();

        services.AddSingleton<ILogger>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
            var melLogger = loggerFactory.CreateLogger("AppFactory");
            return new MicrosoftExtensionsLogger(melLogger);
        });

        return services;
    }

    /// <summary>
    /// Registers AppFactory logging using Microsoft.Extensions.Logging with minimum log level
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="minLogLevel">Minimum log level</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMicrosoftExtensionsLogging(
        this IServiceCollection services,
        LogLevel minLogLevel)
    {
        var melLogLevel = minLogLevel.ToMicrosoftLogLevel();

        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(melLogLevel);
            builder.AddConsole();
            builder.AddDebug();
        });

        services.AddSingleton<ILoggerFactory, MicrosoftExtensionsLoggerFactory>();

        services.AddSingleton<ILogger>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
            var melLogger = loggerFactory.CreateLogger("AppFactory");
            return new MicrosoftExtensionsLogger(melLogger);
        });

        return services;
    }
}
