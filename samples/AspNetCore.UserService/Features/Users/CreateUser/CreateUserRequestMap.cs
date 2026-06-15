using AppFactory.Framework.Api.Parsing.Configurations;

namespace AspNetCore.UserService.Features.Users.CreateUser;

/// <summary>
/// Defines how to map HTTP request data to CreateUserRequest
/// </summary>
public sealed class CreateUserRequestMap : ParseModelMap<CreateUserRequest>
{
    public CreateUserRequestMap()
    {
        Map(x => x.Email, "email").FromBody();
        Map(x => x.Name, "name").FromBody();
    }
}
