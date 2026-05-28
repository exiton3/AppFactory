using Serilog;

namespace AppFactory.Framework.Logging;

public interface ILoggerFactory
{
    ILogger CreateLogger(LoggerConfiguration configuration);
    ILogger CreatePlainTextLogger();
}