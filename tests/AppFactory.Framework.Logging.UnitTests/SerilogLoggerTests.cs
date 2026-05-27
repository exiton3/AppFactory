using AppFactory.Framework.TestExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Serilog.Sinks.XUnit3;
using Xunit;
using Xunit.v3;



namespace AppFactory.Framework.Logging.UnitTests
{
    public class SerilogLoggerTests
    {
        private readonly ITestOutputHelper _testOutput;
        private  SerilogLogger _logger;

        public SerilogLoggerTests(ITestOutputHelper testOutput)
        {
            _testOutput = (TestOutputHelper)testOutput;
            _logger = MakeLogger(testOutput, new PlainTextFormatter());
        }

        private static SerilogLogger MakeLogger(ITestOutputHelper testOutput, ITextFormatter textFormatter)
        {
            var sink = MakeSerilogSink(testOutput, textFormatter);

            return new SerilogLogger(new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(sink));
        }

        private static XUnit3TestOutputSink MakeSerilogSink(ITestOutputHelper testOutput, ITextFormatter? textFormatter = null)
        {
            var sink = new XUnit3TestOutputSink(Options.Create(new XUnit3TestOutputSinkOptions ()))
            {
                TestOutputHelper = testOutput,
                
                //MessageSink = messageSink
            };
            return sink;
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

        [Fact(Skip = "Fix later on Logging with Json Formatter")]
        
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

            services.AddLogging(x => x.LogLevel = LogLevel.Debug , logger => logger.WriteTo.Sink(MakeSerilogSink(_testOutput, new PlainTextFormatter())));

            var provider = services.BuildServiceProvider();

            var logger = provider.GetService<ILogger>();
            var message = new Message { Code = "1234", Number = 5678 };

            logger.LogDebug("Debug {@Message}",message);
            logger.LogInfo("Info {@Message}", message);

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
