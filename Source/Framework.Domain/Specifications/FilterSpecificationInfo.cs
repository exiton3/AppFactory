using System;
using AppFactory.Framework.Domain.Paging;

namespace AppFactory.Framework.Domain.Specifications
{
    class FilterSpecificationInfo<TEntity> where TEntity: class
    {
        public string FieldName { get; set; }

        public Func<IFilter, ISpecification<TEntity>> Factory { get; set; }
         
    }
}