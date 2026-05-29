using AppFactory.Framework.EventBus.Abstractions;
using EventDriven.Aws.UserService.Events;

namespace EventDriven.Aws.UserService.Application.EventHandlers;

public class SendWelcomeEmailHandler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"Sending welcome email to {@event.Data.Email}");
        
        Console.WriteLine($"Welcome email sent to {@event.Data.Email}");
        
        await Task.CompletedTask;
    }
}
