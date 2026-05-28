using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Compact;

namespace AppFactory.Framework.Logging.Serilog;

/// <summary>
/// Extension methods for registering Serilog-based logging in the dependency injection container
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers Serilog logging with configuration from environment variable
    /// </summary>
    public static IServiceCollection AddSerilogLogging(this IServiceCollection services)
    {
        var logLevel = GetLogLevelFromEnvironmentOrDefault();

        services.AddSerilogLogging(x => x.LogLevel = logLevel);

        return services;
    }

    /// <summary>
    /// Registers Serilog logging with the specified configuration
    /// </summary>
    public static IServiceCollection AddSerilogLogging(
        this IServiceCollection services,
        Action<LogConfig> configureLogger)
    {
        services.AddSingleton(_ =>
        {
            var logConfig = new LogConfig();
            configureLogger(logConfig);
            var loggingLevelSwitch = new LoggingLevelSwitch(logConfig.LogLevel.ToSerilogLevel());

            return loggingLevelSwitch;
        });

        // Register Serilog.ILogger
        services.AddSingleton<global::Serilog.ILogger>(provider =>
        {
            var logConfig = new LogConfig();
            configureLogger(logConfig);
            var loggingLevelSwitch = provider.GetRequiredService<LoggingLevelSwitch>();
            return new LoggerConfiguration()
                .MinimumLevel.ControlledBy(loggingLevelSwitch)
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .CreateLogger();
        });

        // Register AppFactory ILogger
        services.AddSingleton<ILogger>(provider =>
        {
            var serilogLogger = provider.GetRequiredService<global::Serilog.ILogger>();
            return new SerilogLogger(serilogLogger);
        });

        // Register ILoggerFactory
        services.AddSingleton<ILoggerFactory>(provider =>
        {
            var serilogLogger = provider.GetRequiredService<global::Serilog.ILogger>();
            return new SerilogLoggerFactory(serilogLogger);
        });

        return services;
    }

    /// <summary>
    /// Registers Serilog logging with custom Serilog configuration
    /// </summary>
    public static IServiceCollection AddSerilogLogging(
        this IServiceCollection services,
        Action<LogConfig> configureLogger,
        Action<LoggerConfiguration> configureSerilog)
    {
        services.AddSingleton(_ =>
        {
            var logConfig = new LogConfig();
            configureLogger(logConfig);
            var loggingLevelSwitch = new LoggingLevelSwitch(logConfig.LogLevel.ToSerilogLevel());

            return loggingLevelSwitch;
        });

        // Register Serilog.ILogger
        services.AddSingleton<global::Serilog.ILogger>(provider =>
        {
            var logConfig = new LogConfig();
            configureLogger(logConfig);
            var loggingLevelSwitch = provider.GetRequiredService<LoggingLevelSwitch>();
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(loggingLevelSwitch);
            configureSerilog(loggerConfiguration);
            return loggerConfiguration.CreateLogger();
        });

        // Register AppFactory ILogger
        services.AddSingleton<ILogger>(provider =>
        {
            var serilogLogger = provider.GetRequiredService<global::Serilog.ILogger>();
            return new SerilogLogger(serilogLogger);
        });

        // Register ILoggerFactory
        services.AddSingleton<ILoggerFactory>(provider =>
        {
            var serilogLogger = provider.GetRequiredService<global::Serilog.ILogger>();
            return new SerilogLoggerFactory(serilogLogger);
        });

        return services;
    }

    private static LogLevel GetLogLevelFromEnvironmentOrDefault()
    {
        var logLevel = Environment.GetEnvironmentVariable("log_level");

        if (string.IsNullOrEmpty(logLevel))
        {
            return LogLevel.Information;
        }

        if (Enum.TryParse(typeof(LogLevel), logLevel, true, out var parsedLogLevel))
        {
            return (LogLevel)parsedLogLevel;
        }

        return LogLevel.Information;
    }
}



