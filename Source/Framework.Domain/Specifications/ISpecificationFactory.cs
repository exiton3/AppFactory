using AppFactory.Framework.Domain.Paging;

namespace AppFactory.Framework.Domain.Specifications
{
    public interface ISpecificationFactory
    {
        ISpecification<T> Create<T>(Filter filter) where T:class ;
    }
}