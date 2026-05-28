using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Domain.ServiceResult;
using AWS.Lambda.UserService.Application.DTOs;

namespace AWS.Lambda.UserService.Application.Processors;

public class CreateUserProcessor : IFunctionProcessor<Commands.CreateUserCommand, UserDto>
{
    private readonly ICommandHandler<Commands.CreateUserCommand> _commandHandler;

    public CreateUserProcessor(ICommandHandler<Commands.CreateUserCommand> commandHandler)
    {
        _commandHandler = commandHandler;
    }

    public async Task<Result<UserDto>> Process(Commands.CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        var result = await _commandHandler.Handle(request, cancellationToken);

        if (result.IsFailure)
        {
            return Result<UserDto>.Invalid(result.Errors);
        }

        var userDto = new UserDto
        {
            Id = result.Id,
            Email = request.Email,
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        return Result<UserDto>.Ok(userDto);
    }
}
