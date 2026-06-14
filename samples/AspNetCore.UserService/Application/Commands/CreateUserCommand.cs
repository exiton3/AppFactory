using AppFactory.Framework.Application.Commands;

namespace AspNetCore.UserService.Application.Commands;

public class CreateUserCommand : ICommand
{
    public string Email { get; set; }
    public string Name { get; set; }
}
