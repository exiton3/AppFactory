using AppFactory.Framework.Api.Parsing.Converters;

namespace AppFactory.Framework.Api.Parsing.Configurations
{
    public sealed class PropertyMapInfo<T> : IPropertyMapInfo
    {
        public Action<T, object> Setter { get; set; }

        Action<object, object> IPropertyMapInfo.Setter
        {
            get { return (o, p) => Setter((T)o, p); }
        }

        public string FieldName { get; set; }
        public string PropertyName { get; set; }

        public ITypeConverter TypeConverter { get; set; }

        public bool IsTypeConverterSet
        {
            get { return TypeConverter != null; }
        }

        public From MapFrom { get; set; }
        public Type PropertyType { get; set; }
        public bool IsRequired { get; set; }
        public BodyContentType ContentType { get; set; }
    }
}