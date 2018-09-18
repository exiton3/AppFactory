using Framework.Domain.Paging;

namespace Framework.Domain.Specifications
{
    public interface ISpecificationFactory
    {
        ISpecification<T> Create<T>(Filter filter) where T:class ;
    }
}