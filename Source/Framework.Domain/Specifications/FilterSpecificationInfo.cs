using System;
using Framework.Domain.Paging;

namespace Framework.Domain.Specifications
{
    class FilterSpecificationInfo<TEntity> where TEntity: class
    {
        public string FieldName { get; set; }

        public Func<IFilter, ISpecification<TEntity>> Factory { get; set; }
         
    }
}