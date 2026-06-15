using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using AppFactory.Framework.Api.AspNetCore.Extensions;

namespace AspNetCore.UserService.Features.Users.CreateUser;

/// <summary>
/// Endpoint configuration for CreateUser feature
/// </summary>
public static class CreateUserEndpoint
{
    public static IEndpointRouteBuilder MapCreateUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCqrsEndpoint<CreateUserRequest, CreateUserResponse>("/api/users", "POST")
            .WithName("CreateUser")
            .WithSummary("Create a new user")
            .WithDescription("Creates a new user with the specified email and name")
            .WithTags("Users")
            .Produces<CreateUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return endpoints;
    }
}
