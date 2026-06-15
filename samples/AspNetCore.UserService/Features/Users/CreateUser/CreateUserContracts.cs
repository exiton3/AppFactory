using AspNetCore.UserService.Contracts.Users;

namespace AspNetCore.UserService.Features.Users.CreateUser;

/// <summary>
/// Request contract for creating a user
/// </summary>
public sealed class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Response contract for creating a user
/// </summary>
public sealed class CreateUserResponse
{
    public UserDto User { get; set; } = null!;
}
