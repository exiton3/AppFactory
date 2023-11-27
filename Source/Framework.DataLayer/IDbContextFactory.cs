using Microsoft.EntityFrameworkCore;

namespace AppFactory.Framework.DataLayer
{
    public interface IDbContextFactory<T> where T:DbContext
    {
        T Create() ;
    }
}