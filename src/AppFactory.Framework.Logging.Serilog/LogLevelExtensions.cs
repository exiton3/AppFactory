using Serilog.Events;

namespace AppFactory.Framework.Logging.Serilog;

/// <summary>
/// Extension methods for converting AppFactory log levels to Serilog log levels
/// </summary>
internal static class LogLevelExtensions
{
    /// <summary>
    /// Converts AppFactory LogLevel to Serilog LogEventLevel
    /// </summary>
    public static LogEventLevel ToSerilogLevel(this LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
    }
}
