using AppFactory.Framework.DataAccess.CosmosDB.Configuration;

namespace AppFactory.Framework.DataAccess.CosmosDB.Mapping;

public interface IValueFormatter
{
    string Format(object value);
    object Parse(string str);
}

public abstract class ValueFormatter<TSource> : IValueFormatter
{
    public string Format(object value)
    {
        return Format((TSource)value);
    }

    object IValueFormatter.Parse(string str)
    {
        return Parse(str);
    }

    protected abstract string Format(TSource source);

    protected abstract TSource Parse(string str);
}

public class PrefixValueFormatter(string prefix) : ValueFormatter<string>
{
    private string Prefix { get; } = prefix + CosmosDbConstants.Separator;

    protected override string Format(string source)
    {
        return $"{Prefix}{source}";
    }

    protected override string Parse(string str)
    {
        if (str.StartsWith(Prefix))
        {
            return str.Substring(Prefix.Length);
        }
        throw new FormatException("Invalid format");
    }
}

