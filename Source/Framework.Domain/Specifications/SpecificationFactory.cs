using System;
using System.Collections.Generic;
using AppFactory.Framework.Domain.Infrastructure;
using AppFactory.Framework.Domain.Paging;

namespace AppFactory.Framework.Domain.Specifications
{
    public class SpecificationFactory : ISpecificationFactory
    {
        private readonly Dictionary<string, Type> _specificationTypesMap = new Dictionary<string, Type>();

        private readonly ICustomSpecificationsRegistry _customSpecificationsRegistry;

        public SpecificationFactory(ICustomSpecificationsRegistry customSpecificationsRegistry)
        {
            _customSpecificationsRegistry = customSpecificationsRegistry;

            _specificationTypesMap.Add(FilterOperands.Equal, typeof(EqualsSpecification<>));
            _specificationTypesMap.Add(FilterOperands.NotEqual, typeof(NotEqualsSpecification<>));
            _specificationTypesMap.Add(FilterOperands.Contains, typeof (ContainsSpecification<>));
            _specificationTypesMap.Add(FilterOperands.DateFrom, typeof (DateFromSpecification<>));
            _specificationTypesMap.Add(FilterOperands.DateTo, typeof (DateToSpecification<>));
        }

        public ISpecification<T> Create<T>(Filter filter) where T : class
        {
            Check.NotNull(filter, nameof(filter));
            if (filter.Operand == FilterOperands.Custom)
            {
                return _customSpecificationsRegistry.GetSpecification<T>(filter);
            }
            if (_specificationTypesMap.ContainsKey(filter.Operand))
            {
                var type = _specificationTypesMap[filter.Operand];

                return CreateSpecificationForFilter<T>(filter, type);
            }

            throw new KeyNotFoundException($"The specification not registered for {filter.Operand} operand");
        }

        private static ISpecification<T> CreateSpecificationForFilter<T>(Filter filter, Type type) where T : class
        {
            if (filter.Values.Count == 1)
                return MakeSpecification<T>(type, filter.FieldName, filter.Values[0].Value);

            ISpecification<T> resultingSpecification = new DefaultFalseSpecification<T>();
            foreach (var filterValue in filter.Values)
            {
                var specficationForFilterValue = MakeSpecification<T>(type, filter.FieldName, filterValue.Value);
                resultingSpecification = new OrSpecification<T>(resultingSpecification, specficationForFilterValue);
            }
            return resultingSpecification;
        }

        private static ISpecification<TModel> MakeSpecification<TModel>(Type type, string fieldName, object value)
            where TModel : class
        {
            var genericType = type.MakeGenericType(typeof(TModel));
            object[] args = {value, fieldName};
            var instance = Activator.CreateInstance(genericType, args);
            return (ISpecification<TModel>) instance;
        }
    }
}