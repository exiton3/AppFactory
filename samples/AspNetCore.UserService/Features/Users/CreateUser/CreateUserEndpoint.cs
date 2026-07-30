using AppFactory.Framework.Api.AspNetCore.Endpoints;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.UserService.Features.Users.CreateUser;

/// <summary>
/// Endpoint configuration for CreateUser feature
/// </summary>
public sealed class CreateUserEndpoint : EndpointConfig<CreateUserRequest, CreateUserResponse>
{
    protected override void Configure()
    {
        Post("/api/users")
            .Name("CreateUser")
            .Summary("Create a new user")
            .Description("Creates a new user with the specified email and name")
            .Tags("Users")
            .Security()
            .AllowAnonymous()
            .Produces<CreateUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
