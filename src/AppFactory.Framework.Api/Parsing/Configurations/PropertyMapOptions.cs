using AppFactory.Framework.Api.Parsing.Converters;

namespace AppFactory.Framework.Api.Parsing.Configurations
{
    public  class PropertyMapOptions<T> : IPropertyMapOptions, IBodyPropertyMapOptions, IQueryPropertyMapOptions, IPathPropertyMapOptions
    {
        private readonly PropertyMapInfo<T> _mapInfo;

        public PropertyMapOptions(PropertyMapInfo<T> mapInfo)
        {
            _mapInfo = mapInfo;
        }

        public IPropertyMapOptions UseConverter<TConverter>() where TConverter : ITypeConverter, new()
        {
            _mapInfo.TypeConverter = new TConverter();

            return this;
        }

        public IQueryPropertyMapOptions FromQuery()
        {
            _mapInfo.MapFrom = From.Query;

            return this;
        }

        public IPathPropertyMapOptions FromPath()
        {
            _mapInfo.MapFrom = From.Path;
            _mapInfo.IsRequired = true;

            return this;
        }

        public IBodyPropertyMapOptions FromBody()
        {
            _mapInfo.MapFrom = From.Body;
            _mapInfo.ContentType = BodyContentType.Json;

            return this;
        }

        public IPropertyMapOptions Required()
        {
            _mapInfo.IsRequired = true;

            return this;
        }

        public IBodyPropertyMapOptions AsText()
        {
            _mapInfo.ContentType = BodyContentType.Text;

            return this;
        }

        public IBodyPropertyMapOptions AsJson()
        {
            _mapInfo.ContentType = BodyContentType.Json;

            return this;
        }

        public IBodyPropertyMapOptions UseContentType(BodyContentType contentType)
        {
            _mapInfo.ContentType = contentType;

            return this;
        }
    }
}