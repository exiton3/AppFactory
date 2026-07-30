using AppFactory.Framework.Api.Parsing.Converters;

namespace AppFactory.Framework.Api.Parsing.Configurations
{
    public interface IQueryPropertyMapOptions : IUseConverterPropertyMapOptions
    {
        IPropertyMapOptions Required();
    }

    public interface IUseConverterPropertyMapOptions
    {
        IPropertyMapOptions UseConverter<TConverter>() where TConverter : ITypeConverter, new();
    }

    public interface IPathPropertyMapOptions : IUseConverterPropertyMapOptions
    {
    }

    public interface IPropertyMapOptions 
    {
        IQueryPropertyMapOptions FromQuery();
        IPathPropertyMapOptions FromPath();
        IBodyPropertyMapOptions FromBody();
    }

    public interface IBodyPropertyMapOptions
    {
        IBodyPropertyMapOptions AsText();
        IBodyPropertyMapOptions AsJson();
        IBodyPropertyMapOptions UseContentType(BodyContentType contentType);
    }
}