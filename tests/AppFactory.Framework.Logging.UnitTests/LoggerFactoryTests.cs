using AppFactory.Framework.TestExtensions;
using Xunit;
using Xunit.Abstractions;

namespace AppFactory.Framework.Logging.UnitTests;

public class LoggerFactoryTests
{
    private readonly ITestOutputHelper _testOutput;
    private LoggerFactory _factory;

    public LoggerFactoryTests(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
        _factory = new LoggerFactory();
    }

    [Fact]
    public void CanWriteToCustom()
    {
        
        var logger = _factory.CreatePlainTextLogger();

        logger.AddTraceId("asdf");
        logger.LogInfo("Some {@Message}", "asdfasdf");

        logger.ShouldBeInstanceOf<SerilogLogger>();
    }
}