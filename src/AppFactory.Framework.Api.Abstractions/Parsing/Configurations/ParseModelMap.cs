using System.Linq.Expressions;
using AppFactory.Framework.Shared;

namespace AppFactory.Framework.Api.Parsing.Configurations
{
    public abstract class ParseModelMap<T> : IParseModelMap where T : new()
    {
        private readonly List<IPropertyMapInfo> _columnMapInfos = new();

        protected IPropertyMapOptions Map<TValue>(Expression<Func<T, TValue>> setterExpression, string name)
        {
            var set = PropertyExpressionHelper.InitializeSetter(setterExpression);
            var setter = new Action<T, object>((o, v) => set(o, (TValue)v));

            var mapInfo = new PropertyMapInfo<T>
            {
                Setter = setter,
                FieldName = name,
                PropertyName = PropertyExpressionHelper.GetPropertyName(setterExpression),
                MapFrom = From.Path,
                PropertyType = typeof(TValue)
            };

            var columnMapOptions = new PropertyMapOptions<T>(mapInfo);

            _columnMapInfos.Add(mapInfo);

            return columnMapOptions;
        }

        protected IPropertyMapOptions Map<TValue>(Expression<Func<T, TValue>> setterExpression)
        {
           var propertyName = PropertyExpressionHelper.GetPropertyName(setterExpression);

           string fieldName = propertyName.ToCamelCase();

            return Map(setterExpression, fieldName);
        }


        public IPropertyMapInfo GetMapInfo(string modelFieldName)
        {
            string columnName = modelFieldName.Trim();
            var columnInfo = _columnMapInfos.FirstOrDefault(x => x.FieldName.Equals(columnName, StringComparison.InvariantCultureIgnoreCase));

            return columnInfo;
        }

        public string GetColumnName(string propertyName)
        {
            return _columnMapInfos.First(x => x.PropertyName == propertyName).FieldName;
        }

        public IEnumerable<IPropertyMapInfo> GetPropertyMappings()
        {
            return _columnMapInfos;
        }
    }
}