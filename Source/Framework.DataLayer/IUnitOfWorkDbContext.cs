using System.Data.Entity;
using Framework.Domain.Repositories;

namespace Framework.DataLayer
{
    public interface IUnitOfWorkDbContext : IUnitOfWork
    {
        DbContext Context { get; }
    }
}