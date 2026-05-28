using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Api.Parsing.Configurations;

namespace AWS.Lambda.UserService.Application.Commands;

public class CreateUserCommand : ICommand
{
    [FromBody]
    public string Email { get; set; }

    [FromBody]
    public string Name { get; set; }
}
