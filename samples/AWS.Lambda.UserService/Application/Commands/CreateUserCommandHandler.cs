using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Domain.Commands;
using AppFactory.Framework.Domain.Repositories;
using AppFactory.Framework.Domain.ServiceResult;
using AWS.Lambda.UserService.Domain;

namespace AWS.Lambda.UserService.Application.Commands;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IRepository<User> _userRepository;

    public CreateUserCommandHandler(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<CommandResult> Handle(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return CommandResult.ErrorResult("EMAIL_REQUIRED", "Email is required");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return CommandResult.ErrorResult("NAME_REQUIRED", "Name is required");
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = command.Email,
            Name = command.Name,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);

        return CommandResult.Success(user.Id);
    }
}
