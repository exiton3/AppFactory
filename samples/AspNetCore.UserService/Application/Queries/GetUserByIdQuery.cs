using AppFactory.Framework.Application.Queries;
using AppFactory.Framework.Api.Parsing.Configurations;

namespace AspNetCore.UserService.Application.Queries;

public class GetUserByIdQuery : IQueryRequest
{
    [FromPath("userId")]
    public string UserId { get; set; }
}
