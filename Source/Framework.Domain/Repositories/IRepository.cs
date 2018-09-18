using System;
using System.Linq;

namespace Framework.Domain.Repositories
{
    public interface IRepository<TEntity> : IRepositoryWithTypeId<TEntity, int> where TEntity : Entity
    {
    }
}