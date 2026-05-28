namespace AppFactory.Framework.Logging;

public interface ILogger
{
    void AddTraceId(string traceId);
    void LogInfo(string message);
    void LogTrace(string message);
    ITimeLogger LogPerformance(string message);
    void LogError(Exception exception, string messageTemplate);
    void LogInfo(string messageTemplate, params object[] propertyValues);
    void LogDebug(string context, object value, string message , params object[] propertyValues);
    void LogDebug(string context, object value, string message);
    void SetContext(string context);
    void LogDebug(string messageTemplate, params object[] values);
    void LogTrace(string message, params object[] propertyValues);
    void LogError(Exception exception, string messageTemplate, params object[] values);
}