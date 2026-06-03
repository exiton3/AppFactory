using Microsoft.Extensions.Logging;

namespace AppFactory.Framework.Logging.MicrosoftExtensions;

/// <summary>
/// Adapter that implements AppFactory.Framework.Logging.ILogger using Microsoft.Extensions.Logging
/// </summary>
internal class MicrosoftExtensionsLogger : ILogger
{
    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private string _traceId = string.Empty;
    private string _context = "traceId";

    public MicrosoftExtensionsLogger(Microsoft.Extensions.Logging.ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddTraceId(string traceId)
    {
        _traceId = traceId;
    }

    public void SetContext(string context)
    {
        _context = context;
    }

    public void LogInfo(string message)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { [_context] = _traceId }))
        {
            _logger.LogInformation(message);
        }
    }

    public void LogInfo(string messageTemplate, params object[] propertyValues)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { [_context] = _traceId }))
        {
            _logger.LogInformation(messageTemplate, propertyValues);
        }
    }

    public void LogTrace(string message)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { [_context] = _traceId }))
        {
            _logger.LogTrace(message);
        }
    }

    public void LogTrace(string message, params object[] propertyValues)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { [_context] = _traceId }))
        {
            _logger.LogTrace(message, propertyValues);
        }
    }

    public void LogDebug(string messageTemplate, params object[] values)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { [_context] = _traceId }))
        {
            _logger.LogDebug(messageTemplate, values);
        }
    }

    public void LogDebug(string context, object value, string message)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { [context] = value }))
        {
            _logger.LogDebug(message);
        }
    }

    public void LogDebug(string context, object value, string messageTemplate, params object[] values)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { [context] = value }))
        {
            _logger.LogDebug(messageTemplate, values);
        }
    }

    public void LogError(Exception exception, string messageTemplate)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { [_context] = _traceId }))
        {
            _logger.LogError(exception, messageTemplate);
        }
    }

    public void LogError(Exception exception, string messageTemplate, params object[] values)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { [_context] = _traceId }))
        {
            _logger.LogError(exception, messageTemplate, values);
        }
    }

    public void LogError(string messageTemplate, params object[] values)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { [_context] = _traceId }))
        {
            _logger.LogError(messageTemplate, values);
        }
    }

    public ITimeLogger LogPerformance(string message)
    {
        return new PerformanceLogger(_logger, message);
    }
}
