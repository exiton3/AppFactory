using AppFactory.Framework.Domain.Repositories;
using AspNetCore.UserService.Domain.Users;
using System.Collections.Concurrent;

namespace AspNetCore.UserService.Infrastructure.Persistence;

/// <summary>
/// In-memory implementation of user repository for demo purposes
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User> GetById<TKey>(TKey key)
    {
        var id = key?.ToString() ?? string.Empty;
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user)!;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<User>>(_users.Values.ToList());
    }

    public Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        _users[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<bool> Add(User model)
    {
        _users[model.Id] = model;
        return Task.FromResult(true);
    }

    public Task<User> UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        _users[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<bool> Update(User model)
    {
        _users[model.Id] = model;
        return Task.FromResult(true);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _users.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<bool> Delete<TKey>(TKey key, CancellationToken cancellationToken = default)
    {
        var id = key?.ToString() ?? string.Empty;
        _users.TryRemove(id, out _);
        return Task.FromResult(true);
    }

    public Task BatchAddItems(IEnumerable<User> models)
    {
        foreach (var model in models)
        {
            _users[model.Id] = model;
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _users.Clear();
    }
}
