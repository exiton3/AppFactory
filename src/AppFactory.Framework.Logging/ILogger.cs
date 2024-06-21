namespace AppFactory.Framework.Logging;

public interface ILogger
{
    void AddTraceId(string traceId);
    void LogInfo(string message);
    void LogTrace(string message);
    ITimeLogger LogPerformance(string message);
    void LogError(Exception exception, string messageTemplate);
    void LogInfo(string messageTemplate, params object[] propertyValues);
}