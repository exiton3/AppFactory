using System.Globalization;

namespace AppFactory.Framework.Api.Parsing.Converters;

public class StringToDecimalConverter : TypeConverter<string, decimal>
{
    protected override decimal Convert(string source)
    {
        return decimal.TryParse(source, out decimal result) ? result : default;
    }

    protected override string ConvertBack(decimal source)
    {
        return source.ToString(CultureInfo.InvariantCulture);
    }
}