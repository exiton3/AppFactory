using AppFactory.Framework.Application.Queries;

namespace AspNetCore.UserService.Features.Users.GetUserById;

/// <summary>
/// Query to get a user by ID
/// </summary>
public sealed class GetUserByIdQuery : IQueryRequest
{
    public string UserId { get; set; } = string.Empty;
}
