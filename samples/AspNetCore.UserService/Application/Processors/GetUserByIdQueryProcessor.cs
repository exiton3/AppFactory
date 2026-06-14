using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Application.Queries;
using AppFactory.Framework.Domain.ServiceResult;
using AspNetCore.UserService.Application.DTOs;

namespace AspNetCore.UserService.Application.Queries;

public class GetUserByIdQueryProcessor : IFunctionProcessor<GetUserByIdQuery, UserDto>
{
    private readonly IQueryHandler<GetUserByIdQuery, UserDto> _queryHandler;

    public GetUserByIdQueryProcessor(IQueryHandler<GetUserByIdQuery, UserDto> queryHandler)
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
