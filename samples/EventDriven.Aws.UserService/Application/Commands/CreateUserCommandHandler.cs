using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Domain.Commands;
using AppFactory.Framework.Domain.Repositories;
using AppFactory.Framework.EventBus.Abstractions;
using EventDriven.Aws.UserService.Domain;
using EventDriven.Aws.UserService.Events;

namespace EventDriven.Aws.UserService.Application.Commands;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IRepository<User> _userRepository;
    private readonly IEventPublisher _eventPublisher;

    public CreateUserCommandHandler(
        IRepository<User> userRepository,
        IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<CommandResult> Handle(CreateUserCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return CommandResult.ErrorResult("EMAIL_REQUIRED", "Email is required");
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = command.Email,
            Name = command.Name,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, ct);

        var @event = new UserCreatedEvent
        {
            Data = new UserCreatedEventData
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                CreatedAt = user.CreatedAt
            }
        };
        
        @event.AddCorrelationId(user.Id);

        await _eventPublisher.PublishAsync(@event, ct);

        return CommandResult.Success(user.Id);
    }
}
