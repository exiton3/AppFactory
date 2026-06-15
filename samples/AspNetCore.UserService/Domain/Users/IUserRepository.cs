using AppFactory.Framework.Domain.Repositories;

namespace AspNetCore.UserService.Domain.Users;

/// <summary>
/// Repository interface for User aggregate
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
