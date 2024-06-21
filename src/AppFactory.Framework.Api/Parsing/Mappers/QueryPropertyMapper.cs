using AppFactory.Framework.Api.Parsing.Configurations;

namespace AppFactory.Framework.Api.Parsing.Mappers;

class QueryPropertyMapper : IPropertyMapper
{
    public bool CanMap(IPropertyMapInfo mapInfo)
    {
        return mapInfo.MapFrom == From.Query;
    }

    public object Map(InputRequest request, IPropertyMapInfo mapInfo)
    {
        if (request.Query.ContainsKey(mapInfo.FieldName))
        {
            object value = request.Query[mapInfo.FieldName];

            if (mapInfo.IsTypeConverterSet)
            {
                value = mapInfo.TypeConverter.Convert(value);
            }

            return value;
        }

        if (mapInfo.IsRequired)
        {
            throw new RequestParsingException($@"The query parameter with the name '{mapInfo.FieldName}', not found in Query String.");

        }
        else
        {
            return null;
        }

    }
}