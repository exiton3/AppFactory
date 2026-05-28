using System.ComponentModel;

namespace AppFactory.Framework.DataAccess.CosmosDB.Mapping;

interface IPropertyMapper
{
    object Map(IPropertyMapInfo mapInfo, object value);

    object MapBack(IPropertyMapInfo mapInfo, object value);
}

internal interface IPropertyMapInfo
{
    Func<object, object> Getter { get; }
    Action<object, object> Setter { get; }

    string DestinationPropertyName { get; set; }

    string SourcePropertyName { get; set; }

    IValueFormatter ValueFormatter { get; set; }
    bool IsValueFormatterSet { get; }
    bool IsTypeConverterSet { get; }
    Type PropertyType { get; set; }
    PropertyKind PropertyKind { get; set; }
}

public enum PropertyKind
{
    Id,
    PartitionKey,
}