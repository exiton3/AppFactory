namespace AspNetCore.UserService.Contracts.Users;

/// <summary>
/// Data transfer object for User
/// </summary>
public class UserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
