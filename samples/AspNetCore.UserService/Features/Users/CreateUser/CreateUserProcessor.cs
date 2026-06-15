using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Domain.ServiceResult;
using AspNetCore.UserService.Contracts.Users;

namespace AspNetCore.UserService.Features.Users.CreateUser;

/// <summary>
/// Processor for CreateUser requests
/// </summary>
public sealed class CreateUserProcessor : IFunctionProcessor<CreateUserRequest, CreateUserResponse>
{
    private readonly ICommandHandler<CreateUserCommand> _commandHandler;

    public CreateUserProcessor(ICommandHandler<CreateUserCommand> commandHandler)
    {
        _commandHandler = commandHandler;
    }

    public async Task<Result<CreateUserResponse>> Process(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var command = new CreateUserCommand
        {
            Email = request.Email,
            Name = request.Name
        };

        var result = await _commandHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            var errors = result.Errors.Select(e => 
                new AppFactory.Framework.Domain.ServiceResult.Error(e.Code, e.Message)).ToList();
            return Result<CreateUserResponse>.Invalid(errors);
        }

        var response = new CreateUserResponse
        {
            User = new UserDto
            {
                Id = result.Id,
                Email = request.Email,
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        return Result<CreateUserResponse>.Ok(response);
    }
}
