using AppFactory.Framework.Domain.Entities;

namespace AppFactory.Framework.Domain.Repositories;

public interface IRepositoryBase<TEntity> : IDisposable where TEntity : Entity
{
    Task Add(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> Update(TEntity entity, CancellationToken cancellation = default);
    Task<TEntity> GetById(string id);

    Task Add(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<bool> Delete(string id);
}