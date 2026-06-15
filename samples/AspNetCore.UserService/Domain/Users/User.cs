using AppFactory.Framework.Domain.Entities;

namespace AspNetCore.UserService.Domain.Users;

/// <summary>
/// User aggregate root representing a user in the system
/// </summary>
public class User : Entity
{
    public string Email { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private User() { } // EF Core

    public static User Create(string email, string name)
    {
        // Domain validation
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        return new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            Name = name,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
