using AppFactory.Framework.TestExtensions;
using Serilog;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace AppFactory.Framework.Logging.UnitTests
{
    public class SerilogLoggerTests
    {
        private readonly TestOutputHelper _testOutput;
        private readonly SerilogLogger _logger;

        public SerilogLoggerTests(ITestOutputHelper testOutput)
        {
            _testOutput = (TestOutputHelper)testOutput;
            _logger = new SerilogLogger(new LoggerConfiguration().WriteTo.TestOutput(testOutput,new PlainTextFormatter()));
        }

        [Fact]
        public void WritePlainTextUsingPlainTextFormatter()
        {
            _logger.AddTraceId("asdf");
            var message = "Some {@Message}";

            _logger.LogInfo(message);

            _testOutput.Output.ShouldContain(message);

        }
    }
}
