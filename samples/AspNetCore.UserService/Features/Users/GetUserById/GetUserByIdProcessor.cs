using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Application.Queries;
using AppFactory.Framework.Domain.ServiceResult;
using AspNetCore.UserService.Contracts.Users;

namespace AspNetCore.UserService.Features.Users.GetUserById;

/// <summary>
/// Processor for GetUserById requests
/// </summary>
public sealed class GetUserByIdProcessor : IFunctionProcessor<GetUserByIdQuery, UserDto>
{
    private readonly IQueryHandler<GetUserByIdQuery, UserDto?> _queryHandler;

    public GetUserByIdProcessor(IQueryHandler<GetUserByIdQuery, UserDto?> queryHandler)
    {
        _queryHandler = queryHandler;
    }

    public async Task<Result<UserDto>> Process(GetUserByIdQuery request, CancellationToken cancellationToken = default)
    {
        var user = await _queryHandler.Handle(request, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.NotFound("User not found");
        }

        return Result<UserDto>.Ok(user);
    }
}
