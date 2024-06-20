using Serilog;
using Serilog.Formatting.Compact;

namespace AppFactory.Framework.Logging;

class SerilogLogger : ILogger
{
    private const string Context = "TraceId";
    private readonly Serilog.Core.Logger _logger;
    private string _traceId;

    public SerilogLogger()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console( new RenderedCompactJsonFormatter())
            .CreateLogger();
    }

    public void AddTraceId(string traceId)
    {
        _traceId = traceId;
    }

    public void LogInfo(string message)
    {
        _logger.ForContext(Context, _traceId).Information(message);
    }
    public void LogInfo(string messageTemplate, params object[] propertyValues)
    {
        _logger.ForContext(Context, _traceId).Information(messageTemplate, propertyValues);
    }

    public void LogTrace(string message)
    {
        _logger.ForContext(Context, _traceId).Debug(message);
    }

    public ITimeLogger LogPerformance(string message)
    {
        return new TimeLogger(this, message);
    }

    public void LogError(Exception exception, string messageTemplate)
    {
        _logger.ForContext(Context, _traceId).Error(exception, messageTemplate);
    }
}