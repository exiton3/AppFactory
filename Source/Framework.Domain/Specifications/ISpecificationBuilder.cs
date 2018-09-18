using Framework.Domain.Paging;

namespace Framework.Domain.Specifications
{
    public interface ISpecificationBuilder
    {
        ISpecification Build<TModel>(FiltersSet filtersSet) where TModel: class;
    }
}