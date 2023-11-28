using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppFactory.Framework.Domain.Repositories
{
    public interface IRepositoryWithTypeId<TEntity, in TId> where TEntity : class
    {
        Task<TEntity> Get(TId id);

        void Add(TEntity entity);

        void AddOrUpdate(TEntity entity);

        void Save();
        Task SaveAsync();
        Task SaveAsync(CancellationToken cancellationToken);

        void Delete(TEntity entity);

        void DeleteById(TId id);

        Task<IEnumerable<TEntity>> GetAll();

        IEnumerable<TEntity> GetList(IEnumerable<TId> idsList);

        void Update(TEntity entity);
    }
}