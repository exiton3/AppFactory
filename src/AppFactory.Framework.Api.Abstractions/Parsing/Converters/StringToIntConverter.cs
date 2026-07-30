using System.Globalization;

namespace AppFactory.Framework.Api.Parsing.Converters;

public class StringToIntConverter : TypeConverter<string, int>
{
    protected override int Convert(string source)
    {
        return int.TryParse(source, out int result) ? result : default;
    }

    protected override string ConvertBack(int source)
    {
        return source.ToString(CultureInfo.InvariantCulture);
    }
}