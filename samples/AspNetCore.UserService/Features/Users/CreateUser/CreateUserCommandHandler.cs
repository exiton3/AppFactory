using AppFactory.Framework.Application.Commands;
using AspNetCore.UserService.Domain.Users;

namespace AspNetCore.UserService.Features.Users.CreateUser;

/// <summary>
/// Handler for CreateUserCommand
/// </summary>
public sealed class CreateUserCommandHandler : CommandHandler<CreateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public override async Task<CommandResult> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingUser != null)
        {
            return CommandResult.Failure("USER_EXISTS", $"User with email {command.Email} already exists");
        }

        // Create domain entity
        var user = User.Create(command.Email, command.Name);

        // Persist
        await _userRepository.Add(user);

        return CommandResult.Success(user.Id);
    }
}
