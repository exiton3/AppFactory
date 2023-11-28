using AppFactory.Framework.Domain.Entities;

namespace AppFactory.Framework.DataLayer
{
    public abstract class Repository<TEntity> : RepositoryWithTypedId<TEntity, int> where TEntity :Entity, new()
    {
        protected Repository(IUnitOfWorkFactory unitOfWork): base(unitOfWork)
        {
        }
    }
}