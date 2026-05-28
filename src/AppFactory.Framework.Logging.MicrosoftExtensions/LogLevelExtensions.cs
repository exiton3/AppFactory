using Microsoft.Extensions.Logging;

namespace AppFactory.Framework.Logging.MicrosoftExtensions;

/// <summary>
/// Extension methods for converting between AppFactory and Microsoft.Extensions.Logging log levels
/// </summary>
internal static class LogLevelExtensions
{
    /// <summary>
    /// Converts AppFactory LogLevel to Microsoft.Extensions.Logging LogLevel
    /// </summary>
    public static Microsoft.Extensions.Logging.LogLevel ToMicrosoftLogLevel(this LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
            LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
    }

    /// <summary>
    /// Converts Microsoft.Extensions.Logging LogLevel to AppFactory LogLevel
    /// </summary>
    public static LogLevel ToAppFactoryLogLevel(this Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        return logLevel switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace => LogLevel.Trace,
            Microsoft.Extensions.Logging.LogLevel.Debug => LogLevel.Debug,
            Microsoft.Extensions.Logging.LogLevel.Information => LogLevel.Information,
            Microsoft.Extensions.Logging.LogLevel.Warning => LogLevel.Warning,
            Microsoft.Extensions.Logging.LogLevel.Error => LogLevel.Error,
            Microsoft.Extensions.Logging.LogLevel.Critical => LogLevel.Critical,
            Microsoft.Extensions.Logging.LogLevel.None => LogLevel.Information,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
    }
}
