using AppFactory.Framework.Api.Parsing.Configurations;

namespace AppFactory.Framework.Api.Parsing.Mappers;

public class PropertyMapperRegistry : IPropertyMapperRegistry
{
    private readonly IEnumerable<IPropertyMapper> _propertyMappers;

    public PropertyMapperRegistry(IEnumerable<IPropertyMapper> propertyMappers)
    {
        _propertyMappers = propertyMappers;
    }
  
    public IPropertyMapper GetMapper(IPropertyMapInfo mapInfo)
    {
        return _propertyMappers.First(x => x.CanMap(mapInfo));
    }
}