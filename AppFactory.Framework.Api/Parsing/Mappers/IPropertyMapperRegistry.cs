using AppFactory.Framework.Api.Parsing.Configurations;

namespace AppFactory.Framework.Api.Parsing.Mappers;

public interface IPropertyMapperRegistry
{
    IPropertyMapper GetMapper(IPropertyMapInfo mapInfo);
}