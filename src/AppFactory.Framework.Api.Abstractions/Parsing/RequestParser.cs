using AppFactory.Framework.Api.Parsing.Configurations;
using AppFactory.Framework.Api.Parsing.Mappers;

namespace AppFactory.Framework.Api.Parsing;

public class RequestParser : IRequestParser
{
    private readonly IParseModelMapRegistry _modelMapRegistry;
    private readonly IPropertyMapperRegistry _propertyMapperRegistry;

    public RequestParser(IParseModelMapRegistry modelMapRegistry, IPropertyMapperRegistry propertyMapperRegistry)
    {
        _modelMapRegistry = modelMapRegistry;
        _propertyMapperRegistry = propertyMapperRegistry;
    }

    public TOutRequest ParseRequest<TOutRequest>(InputRequest inputRequest) where TOutRequest : class, new()
    {
        var modelMap = _modelMapRegistry.Get<TOutRequest>();

        var resultRequest = new TOutRequest();

        foreach (var mapInfo in modelMap.GetPropertyMappings())
        {
            var mapper = _propertyMapperRegistry.GetMapper(mapInfo);

            var property = mapper.Map(inputRequest, mapInfo);

            if (!mapInfo.IsRequired && property == null)
            {
                continue;
            }
            mapInfo.Setter(resultRequest, property);
        }

        return resultRequest;
    }
}