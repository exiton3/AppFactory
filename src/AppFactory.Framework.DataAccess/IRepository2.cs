using AppFactory.Framework.DataAccess.Models;

namespace AppFactory.Framework.DataAccess;

public interface IRepository2<TModel> where TModel : class
{
    Task<TModel> GetById<TKey>(TKey key);
    Task<bool> Add(TModel model);
    Task<bool> Update(TModel model);
    Task BatchAddItems(IEnumerable<TModel> models);
    Task<bool> Delete(PrimaryKey key, CancellationToken cancellationToken = default);
    void Dispose();
}