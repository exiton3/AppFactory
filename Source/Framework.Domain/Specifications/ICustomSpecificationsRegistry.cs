using AppFactory.Framework.Domain.Paging;

namespace AppFactory.Framework.Domain.Specifications
{
    public interface ICustomSpecificationsRegistry
    {
        ISpecification<T> GetSpecification<T>(Filter filter) where T : class;
    }
}