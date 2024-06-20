namespace AppFactory.Framework.Api.Parsing.Configurations;

public interface IParseModelMap
{
    IPropertyMapInfo GetMapInfo(string modelFieldName);
    string GetColumnName(string propertyName);

    IEnumerable<IPropertyMapInfo> GetPropertyMappings();
}