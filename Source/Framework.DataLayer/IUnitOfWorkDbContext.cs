using AppFactory.Framework.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppFactory.Framework.DataLayer
{
    public interface IUnitOfWorkDbContext : IUnitOfWork
    {
        DbContext Context { get; }
    }
}