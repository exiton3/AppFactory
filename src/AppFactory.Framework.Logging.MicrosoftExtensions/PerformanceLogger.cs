using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AppFactory.Framework.Logging.MicrosoftExtensions;

/// <summary>
/// Tracks and logs performance metrics for an operation
/// </summary>
internal class PerformanceLogger : IDisposable
{
    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;

    public PerformanceLogger(Microsoft.Extensions.Logging.ILogger logger, string operationName)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
        _stopwatch = Stopwatch.StartNew();
        
        _logger.LogDebug("Performance tracking started: {OperationName}", operationName);
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        _logger.LogInformation(
            "Performance tracking completed: {OperationName} took {ElapsedMilliseconds}ms",
            _operationName,
            _stopwatch.ElapsedMilliseconds);
    }
}
