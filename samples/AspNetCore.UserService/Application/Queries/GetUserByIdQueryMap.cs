using AppFactory.Framework.Api.Parsing.Configurations;

namespace AspNetCore.UserService.Application.Queries;

/// <summary>
/// Defines how to map HTTP request data to GetUserByIdQuery properties
/// </summary>
public class GetUserByIdQueryMap : ParseModelMap<GetUserByIdQuery>
{
    public GetUserByIdQueryMap()
    {
        // Map path parameter to query property
        Map(x => x.UserId, "userId").FromPath();
    }
}
