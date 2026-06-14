using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Domain.Repositories;

namespace AspNetCore.UserService.Application.Commands;

public class CreateUserCommandHandler : CommandHandler<CreateUserCommand>
{
    private readonly IRepository<Domain.User> _userRepository;

    public CreateUserCommandHandler(IRepository<Domain.User> userRepository)
    {
        _userRepository = userRepository;
    }

    public override async Task<CommandResult> Handle(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return CommandResult.ErrorResult("EMAIL_REQUIRED", "Email is required");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return CommandResult.ErrorResult("NAME_REQUIRED", "Name is required");
        }

        // Create user
        var user = new Domain.User
        {
            Id = Guid.NewGuid().ToString(),
            Email = command.Email,
            Name = command.Name,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Store user
        await _userRepository.Add(user);

        return CommandResult.Success(user.Id);
    }
}
