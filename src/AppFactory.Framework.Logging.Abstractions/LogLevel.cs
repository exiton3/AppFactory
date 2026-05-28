namespace AppFactory.Framework.Logging;

/// <summary>
/// Log levels for AppFactory Framework logging
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Trace level - most detailed logging
    /// </summary>
    Trace,
    
    /// <summary>
    /// Debug level - detailed information for debugging
    /// </summary>
    Debug,
    
    /// <summary>
    /// Information level - general informational messages
    /// </summary>
    Information,
    
    /// <summary>
    /// Warning level - potentially harmful situations
    /// </summary>
    Warning,
    
    /// <summary>
    /// Error level - error events that might still allow the application to continue
    /// </summary>
    Error,
    
    /// <summary>
    /// Critical level - very severe error events that might cause the application to abort
    /// </summary>
    Critical
}
