using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using AppFactory.Framework.Api.AspNetCore.Extensions;
using AspNetCore.UserService.Contracts.Users;

namespace AspNetCore.UserService.Features.Users.GetUserById;

/// <summary>
/// Endpoint configuration for GetUserById feature
/// </summary>
public static class GetUserByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetUserByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCqrsEndpoint<GetUserByIdQuery, UserDto>("/api/users/{userId}", "GET")
            .WithName("GetUser")
            .WithSummary("Get user by ID")
            .WithDescription("Retrieves a user by their unique identifier")
            .WithTags("Users")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }
}
