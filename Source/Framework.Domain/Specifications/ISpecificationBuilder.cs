using AppFactory.Framework.Domain.Paging;

namespace AppFactory.Framework.Domain.Specifications
{
    public interface ISpecificationBuilder
    {
        ISpecification Build<TModel>(FiltersSet filtersSet) where TModel: class;
    }
}