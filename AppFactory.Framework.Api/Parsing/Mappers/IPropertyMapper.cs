using AppFactory.Framework.Api.Parsing.Configurations;

namespace AppFactory.Framework.Api.Parsing.Mappers;

public interface IPropertyMapper
{
    bool CanMap(IPropertyMapInfo mapInfo);
    object Map(InputRequest request, IPropertyMapInfo mapInfo);
}