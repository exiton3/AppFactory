using AppFactory.Framework.Api.Parsing.Converters;

namespace AppFactory.Framework.Api.Parsing.Configurations
{
    public interface IPropertyMapInfo
    {
        Action<object, object> Setter { get; }
        string FieldName { get; set; }
        string PropertyName { get;  }
        ITypeConverter TypeConverter { get; set; }
        bool IsTypeConverterSet { get; }
        From MapFrom { get; set; }
        Type PropertyType { get; set; }
        bool IsRequired { get; set; }
        BodyContentType ContentType { get; set; }
    }
}