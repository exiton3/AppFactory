namespace AppFactory.Framework.Domain.Repositories;

/// <summary>
/// Thrown when an entity is written with a concurrency token that no longer matches the stored version,
/// i.e. it was changed by someone else since it was loaded. Reload the entity and apply the change again.
/// </summary>
public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string entityType, string id)
        : base($"{entityType} '{id}' was modified by another operation since it was loaded. Reload it and retry.")
    {
        EntityType = entityType;
        Id = id;
    }

    public string EntityType { get; }

    public string Id { get; }
}
