using AppFactory.Framework.EventBus.Abstractions;

namespace EventDriven.Aws.UserService.Events;

public class UserCreatedEventData
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserCreatedEvent : DomainEvent<UserCreatedEventData>
{
    public UserCreatedEvent()
    {
        EventType = "com.appfactory.user.created";
        Source = "user-service";
    }
}
