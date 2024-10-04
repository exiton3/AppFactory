using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Reflection.Metadata;

namespace AppFactory.Framework.Logging;

class SerilogLogger : ILogger
{
    private const string DefaultContext = "TraceId";
    private string _context = DefaultContext;
    private readonly Logger _logger;
    private string _traceId;

    public SerilogLogger()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(new LoggingLevelSwitch(LogEventLevel.Debug))
            .WriteTo.Console(new RenderedCompactJsonFormatter())
            .CreateLogger();
    }

    public SerilogLogger(LoggerConfiguration loggerConfiguration)
    {
        _logger = loggerConfiguration
            .WriteTo.Console(new RenderedCompactJsonFormatter())
            .CreateLogger();
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
        _logger.ForContext(_context, _traceId).Information(message);
    }
    public void LogInfo(string messageTemplate, params object[] propertyValues)
    {
        _logger.Information(messageTemplate, propertyValues);
    }

    public void LogTrace(string message)
    {
        _logger.ForContext(_context, _traceId).Verbose(message);
    }

    public void LogTrace(string message, params object[] propertyValues)
    {
        _logger.Verbose(message, propertyValues);
    }

    public void LogDebug(string messageTemplate, params object[] values)
    {
        _logger.Debug(messageTemplate, values);
    }
    public void LogDebug(string context, object value, string messageTemplate, params object[] values)
    {
        _logger.ForContext(context, value, true).Debug(messageTemplate, values);
    }

    public void LogDebug(string context, object value, string message)
    {
        _logger.ForContext(context, value, true).Debug(message);
    }

    public ITimeLogger LogPerformance(string message)
    {
        return new TimeLogger(this, message);
    }

    public void LogError(Exception exception, string messageTemplate)
    {
        _logger.ForContext(_context, _traceId).Error(exception, messageTemplate);
    }

    public void LogError(Exception exception, string messageTemplate, params object[] values)
    {
        _logger.Error(exception, messageTemplate, values);
    }
}