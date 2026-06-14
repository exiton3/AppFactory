using AppFactory.Framework.Application.Queries;

namespace AspNetCore.UserService.Application.Queries;

public class GetUserByIdQuery : IQueryRequest
{
    public string UserId { get; set; }
}
