using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

namespace AppFactory.Framework.Logging;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddLogging(this IServiceCollection serviceCollection)
    {
        var logLevel = GetLogLevelFromEnvironmentOrDefault();

        serviceCollection.AddLogging(x => x.LogLevel = logLevel);
        
        return serviceCollection;
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

    public static IServiceCollection AddLogging(this IServiceCollection serviceCollection, Action<LogConfig> configureLogger)
    {
        serviceCollection.AddSingleton(_ =>
        {
            var logConfig = new LogConfig();
            configureLogger(logConfig);
            var loggingLevelSwitch = new LoggingLevelSwitch(logConfig.LogLevel.ToSerilogLevel());

            return loggingLevelSwitch;
        });

        serviceCollection.AddSingleton<ILogger>(provider =>
        {
            
            var logConfig = new LogConfig();
            configureLogger(logConfig);
            var loggingLevelSwitch = provider.GetService<LoggingLevelSwitch>();
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(loggingLevelSwitch);
           
            return new SerilogLogger(loggerConfiguration);
        });

        return serviceCollection;
    }

    internal static IServiceCollection AddLogging(this IServiceCollection serviceCollection, Action<LogConfig> configureLogger, Action<LoggerConfiguration> serilogConfig)
    {
        serviceCollection.AddSingleton(_ =>
        {
            var logConfig = new LogConfig();
            configureLogger(logConfig);
            var loggingLevelSwitch = new LoggingLevelSwitch(logConfig.LogLevel.ToSerilogLevel());

            return loggingLevelSwitch;
        });

        serviceCollection.AddSingleton<ILogger>(provider =>
        {

            var logConfig = new LogConfig();
            configureLogger(logConfig);
            var loggingLevelSwitch = provider.GetService<LoggingLevelSwitch>();
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(loggingLevelSwitch);
            serilogConfig(loggerConfiguration);
            return new SerilogLogger(loggerConfiguration);
        });

        return serviceCollection;
    }



}


