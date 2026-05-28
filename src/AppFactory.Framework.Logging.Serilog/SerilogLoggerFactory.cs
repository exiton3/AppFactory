using Serilog;

namespace AppFactory.Framework.Logging.Serilog;

/// <summary>
/// Serilog implementation of ILoggerFactory
/// </summary>
public class SerilogLoggerFactory : ILoggerFactory
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
