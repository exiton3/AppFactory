using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using AppFactory.Framework.Api.AspNetCore.Endpoints;
using AspNetCore.UserService.Contracts.Users;

namespace AspNetCore.UserService.Features.Users.GetUserById;

/// <summary>
/// Endpoint configuration for GetUserById feature
/// </summary>
public sealed class GetUserByIdEndpoint : EndpointConfig<GetUserByIdQuery, UserDto>
{
    protected override void Configure()
    {
        Get("/api/users/{userId}")
            .Name("GetUser")
            .Summary("Get user by ID")
            .Description("Retrieves a user by their unique identifier")
            .Tags("Users")
            .Security()
            .AllowAnonymous()
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}
