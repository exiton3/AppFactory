using Serilog;

namespace AppFactory.Framework.Logging.Serilog;

/// <summary>
/// Serilog implementation of ILoggerFactory
/// </summary>
public class SerilogLoggerFactory : ILoggerFactory
{
    private readonly global::Serilog.ILogger _baseLogger;

    public SerilogLoggerFactory(global::Serilog.ILogger baseLogger)
    {
        _baseLogger = baseLogger;
    }

    public ILogger CreateLogger(string categoryName)
    {
        var contextLogger = _baseLogger.ForContext("SourceContext", categoryName);
        return new SerilogLogger(contextLogger);
    }

    public ILogger CreateLogger<T>()
    {
        var contextLogger = _baseLogger.ForContext<T>();
        return new SerilogLogger(contextLogger);
    }

    /// <summary>
    /// Creates a logger from a LoggerConfiguration
    /// </summary>
    public static ILogger CreateLogger(LoggerConfiguration configuration)
    {
        return new SerilogLogger(configuration);
    }

    /// <summary>
    /// Creates a plain text logger for console output
    /// </summary>
    public static ILogger CreatePlainTextLogger()
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(new PlainTextFormatter());

        return new SerilogLogger(config);
    }
}
