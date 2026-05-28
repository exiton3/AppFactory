using Microsoft.Extensions.Logging;

namespace AppFactory.Framework.Logging.MicrosoftExtensions;

/// <summary>
/// Adapter that implements AppFactory.Framework.Logging.ILogger using Microsoft.Extensions.Logging
/// </summary>
internal class MicrosoftExtensionsLogger : ILogger
{
    private readonly Microsoft.Extensions.Logging.ILogger _logger;

    public MicrosoftExtensionsLogger(Microsoft.Extensions.Logging.ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogTrace(string message)
    {
        _logger.LogTrace(message);
    }

    public void LogDebug(string message)
    {
        _logger.LogDebug(message);
    }

    public void LogInformation(string message)
    {
        _logger.LogInformation(message);
    }

    public void LogWarning(string message)
    {
        _logger.LogWarning(message);
    }

    public void LogError(string message)
    {
        _logger.LogError(message);
    }

    public void LogError(Exception exception, string message)
    {
        _logger.LogError(exception, message);
    }

    public void LogCritical(string message)
    {
        _logger.LogCritical(message);
    }

    public IDisposable LogPerformance(string operationName)
    {
        return new PerformanceLogger(_logger, operationName);
    }
}
