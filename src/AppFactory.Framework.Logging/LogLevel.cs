using Serilog.Events;

namespace AppFactory.Framework.Logging;

public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warring,
    Error,
    Critical
}

public static class LogLevelExtension
{
    public static LogEventLevel ToSerilogLevel(this LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warring => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
    }
}