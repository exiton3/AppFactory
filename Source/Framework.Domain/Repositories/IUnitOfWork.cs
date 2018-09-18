using System;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        void Save();

        Task<int> SaveAsync();
        Task<int> SaveAsync(CancellationToken cancellationToken);
    }
}