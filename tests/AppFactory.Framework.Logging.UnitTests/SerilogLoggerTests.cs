using AppFactory.Framework.TestExtensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace AppFactory.Framework.Logging.UnitTests
{
    public class SerilogLoggerTests
    {
        private readonly TestOutputHelper _testOutput;
        private  SerilogLogger _logger;

        public SerilogLoggerTests(ITestOutputHelper testOutput)
        {
            _testOutput = (TestOutputHelper)testOutput;
            _logger = MakeLogger(testOutput, new PlainTextFormatter());
        }

        private static SerilogLogger MakeLogger(ITestOutputHelper testOutput, ITextFormatter textFormatter)
        {
            return new SerilogLogger(new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.TestOutput(testOutput,textFormatter));
        }

        [Fact]
        public void WritePlainTextUsingPlainTextFormatter()
        {
            _logger.AddTraceId("asdf");
            var message = "Some {@Message}";
            _logger = MakeLogger(_testOutput, new PlainTextFormatter());
            _logger.LogInfo(message);

            _testOutput.Output.ShouldContain(message);

        }

        [Fact]
        public void WriteParametrizedTextUsingPlainTextFormatter()
        {
            _logger.AddTraceId("asdf");
            var message = "Some {@Message}";

            var message1 = new Message { Code = "1234", Number = 5678 };

            _logger = MakeLogger(_testOutput, new CompactJsonFormatter());
            _logger.LogTrace(message, message1);

            _testOutput.Output.ShouldContain(message);

        }

        [Fact]
        public void RegisterWithLogConfig()
        {
            var services = new ServiceCollection();

            services.AddLogging(x => x.LogLevel = LogLevel.Debug , logger => logger.WriteTo.TestOutput(_testOutput, new CompactJsonFormatter()));

            var provider = services.BuildServiceProvider();

            var logger = provider.GetService<ILogger>();
            var message = new Message { Code = "1234", Number = 5678 };

            logger.LogDebug("Debug {@Message}",message);
           // logger.LogInfo("Info {@Message}", message);

            //_testOutput.Output.ShouldBeEmpty();
            var levelSwitch = provider.GetService<LoggingLevelSwitch>();
            levelSwitch.MinimumLevel = LogEventLevel.Error;
            logger.LogInfo("Info {@Message}", message);
            logger.LogError(new AbandonedMutexException(),"Error {@Message}", message);
            logger.LogDebug("Debug {@Message}", message);
            logger.LogInfo("Info {@Message}", message);
            _testOutput.Output.ShouldContain("Error");
        }
    }

    class Message
    {
        public string Code { get; set; }

        public int Number { get; set; }
    }
}
