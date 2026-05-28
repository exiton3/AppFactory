namespace AppFactory.Framework.Logging;

/// <summary>
/// Logging configuration options
/// </summary>
public class LogConfig
{
    /// <summary>
    /// Gets or sets the minimum log level
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}
