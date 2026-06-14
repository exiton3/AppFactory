using AppFactory.Framework.Api.Parsing.Configurations;

namespace AspNetCore.UserService.Application.Commands;

/// <summary>
/// Defines how to map HTTP request data to CreateUserRequest properties
/// </summary>
public class CreateUserRequestMap : ParseModelMap<CreateUserRequest>
{
    public CreateUserRequestMap()
    {
        // Map the entire request body to the Command property
        Map(x => x.Command).FromBody();
    }
}
