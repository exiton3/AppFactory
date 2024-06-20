using System.Diagnostics;

namespace AppFactory.Framework.Logging;

class TimeLogger :ITimeLogger
{
    private readonly ILogger _performanceLogger;
    private readonly Stopwatch _stopwatch;
    private readonly string _stepName;

    public TimeLogger(ILogger logger, string stepName)
    {
        _performanceLogger = logger;
        _stepName = stepName;
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }
    public void Dispose()
    {
        _stopwatch.Stop();

        var time = _stopwatch.ElapsedMilliseconds/1000 == 0?
            _stopwatch.ElapsedMilliseconds +" ms" 
            : _stopwatch.Elapsed.Seconds +" sec";

        _performanceLogger.LogTrace($"{_stepName} executed in {time}");
    }
}