namespace AppFactory.Framework.Domain.Repositories;

public interface IRepository<TModel> where TModel : class
{
    Task<TModel> GetById<TKey>(TKey key);
    Task<bool> Add(TModel model);
    Task<bool> Update(TModel model);
    Task BatchAddItems(IEnumerable<TModel> models);
    Task<bool> Delete<TKey>(TKey key, CancellationToken cancellationToken = default);
    void Dispose();
}