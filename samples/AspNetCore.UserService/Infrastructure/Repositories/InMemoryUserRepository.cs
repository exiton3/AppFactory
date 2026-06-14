using AppFactory.Framework.Domain.Repositories;

namespace AspNetCore.UserService.Infrastructure.Repositories;

/// <summary>
/// In-memory repository implementation for demo purposes.
/// Not suitable for production use - data is lost on application restart.
/// </summary>
public class InMemoryUserRepository : IRepository<Domain.User>
{
    private readonly Dictionary<string, Domain.User> _users = new();
    private readonly object _lock = new();

    public Task<Domain.User> GetById<TKey>(TKey key)
    {
        var userId = key?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromResult<Domain.User>(null!);
        }

        lock (_lock)
        {
            return Task.FromResult(_users.TryGetValue(userId, out var user) ? user : null!);
        }
    }

    public Task<bool> Add(Domain.User model)
    {
        if (model == null || string.IsNullOrEmpty(model.Id))
        {
            return Task.FromResult(false);
        }

        lock (_lock)
        {
            _users[model.Id] = model;
            return Task.FromResult(true);
        }
    }

    public Task<bool> Update(Domain.User model)
    {
        if (model == null || string.IsNullOrEmpty(model.Id))
        {
            return Task.FromResult(false);
        }

        lock (_lock)
        {
            if (!_users.ContainsKey(model.Id))
            {
                return Task.FromResult(false);
            }

            _users[model.Id] = model;
            return Task.FromResult(true);
        }
    }

    public Task BatchAddItems(IEnumerable<Domain.User> models)
    {
        if (models == null)
        {
            return Task.CompletedTask;
        }

        lock (_lock)
        {
            foreach (var model in models)
            {
                if (model != null && !string.IsNullOrEmpty(model.Id))
                {
                    _users[model.Id] = model;
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> Delete<TKey>(TKey key, CancellationToken cancellationToken = default)
    {
        var userId = key?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromResult(false);
        }

        lock (_lock)
        {
            return Task.FromResult(_users.Remove(userId));
        }
    }

    public void Dispose()
    {
        // Nothing to dispose for in-memory storage
    }
}
