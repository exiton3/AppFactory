namespace AppFactory.Framework.Api.Parsing.Converters;

public abstract class TypeConverter<TSource, TDestination> : ITypeConverter
{
    public object Convert(object value)
    {
        return Convert((TSource)value);
    }

    public object ConvertBack(object value)
    {
        return ConvertBack((TDestination)value);
    }

    public Type SourceType { get { return typeof(TSource); } }
    public Type DestinationType { get { return typeof(TDestination); } }

    protected abstract TDestination Convert(TSource source);
    protected abstract TSource ConvertBack(TDestination source);
}