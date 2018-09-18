using Framework.Domain.Paging;

namespace Framework.Domain.Specifications
{
    public interface ICustomSpecificationsRegistry
    {
        ISpecification<T> GetSpecification<T>(Filter filter) where T : class;
    }
}