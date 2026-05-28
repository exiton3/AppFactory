using Serilog.Events;
using Serilog.Formatting;

namespace AppFactory.Framework.Logging.Serilog;

class PlainTextFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        var text = logEvent.MessageTemplate.Text;

        output.WriteLine(text);
    }
}
