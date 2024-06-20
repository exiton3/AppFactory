using AppFactory.Framework.Api.Parsing.Configurations;
using AppFactory.Framework.Infrastructure.Serialization;

namespace AppFactory.Framework.Api.Parsing.Mappers;

class BodyPropertyMapper : IPropertyMapper
{
    private readonly IJsonSerializer _jsonSerializer;

    public BodyPropertyMapper(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public bool CanMap(IPropertyMapInfo mapInfo)
    {
        return mapInfo.MapFrom == From.Body;
    }

    public object Map(InputRequest request, IPropertyMapInfo mapInfo)
    {
        if (mapInfo.ContentType == BodyContentType.Text)
        {
           return request.Body;
        }

        var result = _jsonSerializer.Deserialize(request.Body, mapInfo.PropertyType);

        return result;
    }
}