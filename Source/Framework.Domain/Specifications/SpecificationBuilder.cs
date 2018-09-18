using System.Collections.Generic;
using System.Linq;
using Framework.Domain.Paging;

namespace Framework.Domain.Specifications
{
    public class SpecificationBuilder : ISpecificationBuilder
    {
       private readonly ISpecificationFactory _specificationFactory;

        public SpecificationBuilder(ISpecificationFactory specificationFactory)
        {
            _specificationFactory = specificationFactory;
        }

        public ISpecification Build<TModel>(FiltersSet filtersSet) where TModel: class 
        {
            ISpecification<TModel> resultSpecification = new DefaultSpecification<TModel>();

            var propertyNames = typeof(TModel).GetProperties().Select(x=>x.Name).ToList();

            foreach (var filter in filtersSet.Filters)
            {
                if (!IsPropertyExists(filter.FieldName, propertyNames)) continue;

                var specification = _specificationFactory.Create<TModel>(filter);

                resultSpecification = resultSpecification.And(specification);
            }
            return resultSpecification;
        }

        private bool IsPropertyExists(string filterFieldName, IEnumerable<string> propertyNames)
        {
            return propertyNames.Contains(filterFieldName);
        }

    }
}