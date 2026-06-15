using AppFactory.Framework.Api.Parsing.Configurations;

namespace AspNetCore.UserService.Features.Users.GetUserById;

/// <summary>
/// Defines how to map HTTP request data to GetUserByIdQuery
/// </summary>
public sealed class GetUserByIdQueryMap : ParseModelMap<GetUserByIdQuery>
{
    public GetUserByIdQueryMap()
    {
        Map(x => x.UserId, "userId").FromPath();
    }
}
