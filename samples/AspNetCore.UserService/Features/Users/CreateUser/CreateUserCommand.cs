using AppFactory.Framework.Application.Commands;

namespace AspNetCore.UserService.Features.Users.CreateUser;

/// <summary>
/// Command to create a new user
/// </summary>
public sealed class CreateUserCommand : ICommand
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
