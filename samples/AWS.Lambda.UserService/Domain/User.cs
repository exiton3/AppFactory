using AppFactory.Framework.Domain.Entities;

namespace AWS.Lambda.UserService.Domain;

public class User : Entity
{
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
