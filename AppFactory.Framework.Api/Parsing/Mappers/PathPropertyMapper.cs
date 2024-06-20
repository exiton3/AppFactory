using AppFactory.Framework.Api.Parsing.Configurations;

namespace AppFactory.Framework.Api.Parsing.Mappers;

class PathPropertyMapper : IPropertyMapper
{
    public bool CanMap(IPropertyMapInfo mapInfo)
    {
        return mapInfo.MapFrom == From.Path;
    }

    public object Map(InputRequest request, IPropertyMapInfo mapInfo)
    {
        if (request.Path.ContainsKey(mapInfo.FieldName))
        {
            object value = request.Path[mapInfo.FieldName];

            if (mapInfo.IsRequired)
            {
                if (string.IsNullOrWhiteSpace(value.ToString()))
                {
                    throw new RequestParsingException($"The path parameter '{mapInfo.FieldName}', must not be empty.");
                }
            }

            if (mapInfo.IsTypeConverterSet)
            {
                value = mapInfo.TypeConverter.Convert(value);
            }

            return value;
        }

        throw new RequestParsingException($@"The path parameter '{mapInfo.FieldName}', not found in Path parameters.");
        
    }
}