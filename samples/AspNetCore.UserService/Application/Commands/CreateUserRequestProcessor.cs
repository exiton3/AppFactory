using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Domain.ServiceResult;
using AspNetCore.UserService.Application.DTOs;

namespace AspNetCore.UserService.Application.Commands;

public class CreateUserRequestProcessor : IFunctionProcessor<CreateUserRequest, UserDto>
{
    private readonly ICommandHandler<CreateUserCommand> _commandHandler;

    public CreateUserRequestProcessor(ICommandHandler<CreateUserCommand> commandHandler)
    {
        _commandHandler = commandHandler;
    }

    public async Task<Result<UserDto>> Process(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _commandHandler.Handle(request.Command, cancellationToken);

        if (result.IsFailure)
        {
            var errors = result.Errors.Select(e => new AppFactory.Framework.Domain.ServiceResult.Error(e.Code, e.Message)).ToList();
            return Result<UserDto>.Invalid(errors);
        }

        var userDto = new UserDto
        {
            Id = result.Id,
            Email = request.Command.Email,
            Name = request.Command.Name,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        return Result<UserDto>.Ok(userDto);
    }
}
