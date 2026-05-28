using System.ComponentModel;

namespace AppFactory.Framework.DataAccess.CosmosDB.Mapping;

interface IPropertyMapper
{
    object Map(IPropertyMapInfo mapInfo, object value);

    object MapBack(IPropertyMapInfo mapInfo, object value);
}

class IdKeyPropertyMapper : IPropertyMapper
{
    public object Map(IPropertyMapInfo mapInfo, object value)
    {
        throw new NotImplementedException();
    }

    public object MapBack(IPropertyMapInfo mapInfo, object value)
    {
        throw new NotImplementedException();
    }
}

internal interface IPropertyMapInfo
{
    Func<object, object> Getter { get; }
    Action<object, object> Setter { get; }
    IValueFormatter ValueFormatter { get; set; }
    bool IsValueFormatterSet { get; }
    
    bool IsTypeConverterSet { get; }
    Type PropertyType { get; set; }
    PropertyKind PropertyKind { get; set; }
    string DiscriminatorField { get; set; }
    Dictionary<string, Type> DiscriminatorTypes { get; }
    bool IsDiscriminatorSet { get; }
}

public enum PropertyKind
{
    Value,
    Reference,
    Collection,
    Nullable,
    Array,
    Dictionary
}