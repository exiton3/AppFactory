using Serilog.Events;
using Serilog.Formatting;

namespace AppFactory.Framework.Logging;

class PlainTextFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        var text = logEvent.MessageTemplate.Text;

        output.WriteLine(text);
    }
}