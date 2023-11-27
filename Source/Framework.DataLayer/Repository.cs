using AppFactory.Framework.Domain.Entities;

namespace AppFactory.Framework.DataLayer
{
    public abstract class Repository<TEntity> : RepositoryWithTypedId<TEntity, int> where TEntity :Entity
    {
        protected Repository(IUnitOfWorkFactory unitOfWork): base(unitOfWork)
        {
        }
    }
}