using Framework.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Framework.DataLayer
{
    public interface IUnitOfWorkDbContext : IUnitOfWork
    {
        DbContext Context { get; }
    }
}