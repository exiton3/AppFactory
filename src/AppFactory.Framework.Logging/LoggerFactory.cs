using Serilog;

namespace AppFactory.Framework.Logging;

public class LoggerFactory : ILoggerFactory
{
    public ILogger CreateLogger(LoggerConfiguration configuration)
    {
        return new SerilogLogger(configuration);
    }

    public ILogger CreatePlainTextLogger()
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(new PlainTextFormatter());

        return new SerilogLogger(config);
    }
}