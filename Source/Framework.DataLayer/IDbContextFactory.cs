using System.Data.Entity;

namespace Framework.DataLayer
{
    public interface IDbContextFactory<T> where T:DbContext
    {
        T Create() ;
    }
}