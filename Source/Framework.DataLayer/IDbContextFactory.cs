using Microsoft.EntityFrameworkCore;

namespace Framework.DataLayer
{
    public interface IDbContextFactory<T> where T:DbContext
    {
        T Create() ;
    }
}