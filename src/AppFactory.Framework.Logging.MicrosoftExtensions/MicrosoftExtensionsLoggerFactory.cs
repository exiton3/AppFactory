using Microsoft.Extensions.Logging;

namespace AppFactory.Framework.Logging.MicrosoftExtensions;

/// <summary>
/// Factory for creating AppFactory ILogger instances using Microsoft.Extensions.Logging
/// </summary>
internal class MicrosoftExtensionsLoggerFactory : ILoggerFactory
{
    private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

    public MicrosoftExtensionsLoggerFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public ILogger CreateLogger(string categoryName)
    {
        var melLogger = _loggerFactory.CreateLogger(categoryName);
        return new MicrosoftExtensionsLogger(melLogger);
    }

    public ILogger CreateLogger<T>()
    {
        var melLogger = _loggerFactory.CreateLogger<T>();
        return new MicrosoftExtensionsLogger(melLogger);
    }
}
