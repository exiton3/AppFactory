namespace AppFactory.Framework.Logging;

/// <summary>
/// Factory for creating logger instances
/// </summary>
public interface ILoggerFactory
{
    /// <summary>
    /// Creates a logger with the specified category name
    /// </summary>
    /// <param name="categoryName">The category name for the logger</param>
    /// <returns>A logger instance</returns>
    ILogger CreateLogger(string categoryName);

    /// <summary>
    /// Creates a logger for the specified type
    /// </summary>
    /// <typeparam name="T">The type to create a logger for</typeparam>
    /// <returns>A logger instance</returns>
    ILogger CreateLogger<T>();
}
