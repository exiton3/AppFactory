# AppFactory Event-Driven Architecture Guide

## 🎯 Overview

AppFactory v10.4.0 introduces comprehensive event-driven architecture support, enabling you to build decoupled, scalable microservices that communicate through events across AWS, Azure, and on-premises platforms.

## 🏗️ Architecture

### Event-Driven Pattern

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│   Service   │         │ Event Bus    │         │  Service B  │
│      A      │────────▶│              │────────▶│             │
│  (Publisher)│  Event  │ EventBridge  │  Event  │  (Consumer) │
└─────────────┘         │  Event Grid  │         └─────────────┘
                        │   RabbitMQ   │
                        └──────────────┘
```

### Benefits

- **Decoupling** - Services don't need to know about each other
- **Scalability** - Each service scales independently
- **Resilience** - Failure in one service doesn't affect others
- **Flexibility** - Easy to add new consumers without changing publishers
- **Audit Trail** - Events provide complete history of system changes

## 📦 Core Abstractions

### IEvent - Base Event Interface

```csharp
public interface IEvent
{
    string EventId { get; }           // Unique identifier
    string EventType { get; }         // Event type (e.g., "user.created")
    DateTime OccurredAt { get; }      // When the event occurred
    string Source { get; }            // Source service
    int Version { get; }              // Event schema version
    IDictionary<string, string> Metadata { get; } // Additional metadata
}
```

### IEventPublisher - Publish Events

```csharp
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IEvent;
    
    Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken ct = default)
        where TEvent : IEvent;
}
```

### IEventHandler - Handle Events

```csharp
public interface IEventHandler<TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
```

## 🌩️ CloudEvents Standard

AppFactory supports the [CloudEvents](https://cloudevents.io/) specification v1.0:

```csharp
public class CloudEvent : IEvent
{
    [JsonPropertyName("id")]
    public string EventId { get; set; }

    [JsonPropertyName("type")]
    public string EventType { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("time")]
    public DateTime OccurredAt { get; set; }

    [JsonPropertyName("specversion")]
    public string SpecVersion { get; set; } = "1.0";

    [JsonPropertyName("datacontenttype")]
    public string DataContentType { get; set; } = "application/json";

    [JsonPropertyName("data")]
    public object Data { get; set; }
}
```

### Benefits of CloudEvents
- ✅ Industry standard
- ✅ Cross-platform interoperability
- ✅ Tool support (logging, monitoring, tracing)
- ✅ Schema evolution support

## 🎨 Domain Events

For strongly-typed events, use `DomainEvent<TData>`:

```csharp
public class UserCreatedEventData
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}

public class UserCreatedEvent : DomainEvent<UserCreatedEventData>
{
    public UserCreatedEvent()
    {
        EventType = "com.appfactory.user.created";
        Source = "user-service";
    }
}

// Usage
var @event = new UserCreatedEvent
{
    Data = new UserCreatedEventData
    {
        UserId = "user-123",
        Email = "john@example.com",
        Name = "John Doe"
    }
};

// Add distributed tracing metadata
@event.AddCorrelationId("correlation-123");
@event.AddCausationId("command-456");
@event.AddUserId("admin-789");
```

## 🔄 Publishing Events

### Single Event

```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IRepository<User> _userRepo;
    private readonly IEventPublisher _eventPublisher;

    public async Task<CommandResult> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        // 1. Save to database
        var user = new User { Email = cmd.Email, Name = cmd.Name };
        await _userRepo.AddAsync(user, ct);

        // 2. Publish event
        var @event = new UserCreatedEvent
        {
            Data = new UserCreatedEventData
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name
            }
        };

        await _eventPublisher.PublishAsync(@event, ct);

        return CommandResult.Success(user.Id);
    }
}
```

### Batch Publishing

```csharp
var events = users.Select(u => new UserCreatedEvent
{
    Data = new UserCreatedEventData { UserId = u.Id, Email = u.Email }
}).ToList();

await _eventPublisher.PublishBatchAsync(events, ct);
```

## 🎧 Handling Events

### Event Handler Implementation

```csharp
public class SendWelcomeEmailHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;

    public SendWelcomeEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
    {
        await _emailService.SendWelcomeEmailAsync(
            @event.Data.Email,
            @event.Data.Name,
            ct);
    }
}
```

### AWS Lambda Event Handler

```csharp
using AppFactory.Framework.EventBus.Aws;
using Amazon.Lambda.CloudWatchEvents;
using Amazon.Lambda.Core;

public class WelcomeEmailLambda : LambdaEventHandlerBase<UserCreatedEvent>
{
    protected override IStartup GetStartup() => new Startup();

    public async Task FunctionHandler(
        CloudWatchEvent<UserCreatedEvent> @event,
        ILambdaContext context)
    {
        await base.FunctionHandler(@event, context);
    }
}
```

**serverless.yml:**
```yaml
functions:
  welcomeEmail:
    handler: MyService::MyService.WelcomeEmailLambda::FunctionHandler
    events:
      - eventBridge:
          eventBus: ${self:custom.eventBusName}
          pattern:
            source:
              - user-service
            detail-type:
              - com.appfactory.user.created
```

### Azure Functions Event Handler

```csharp
using AppFactory.Framework.EventBus.Azure;
using Microsoft.Azure.Functions.Worker;

public class WelcomeEmailFunction : AzureFunctionEventHandlerBase<UserCreatedEvent>
{
    protected override IStartup GetStartup() => new Startup();

    [Function("WelcomeEmail")]
    public async Task Run(
        [EventGridTrigger] string eventGridEvent,
        FunctionContext context)
    {
        await base.Run(eventGridEvent, context);
    }
}
```

## 🔧 Dependency Injection Setup

### AWS EventBridge

```csharp
public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register CQRS
        services.AddCqrs(typeof(Startup).Assembly);

        // Register EventBridge publisher
        var eventBusName = Environment.GetEnvironmentVariable("EVENT_BUS_NAME") ?? "default";
        services.AddEventBridgePublisher(eventBusName);

        // Register event handlers
        services.AddEventHandlers(typeof(Startup).Assembly);
    }
}
```

### Azure Event Grid

```csharp
public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register CQRS
        services.AddCqrs(typeof(Startup).Assembly);

        // Register Event Grid publisher
        var endpoint = Environment.GetEnvironmentVariable("EVENT_GRID_ENDPOINT");
        var accessKey = Environment.GetEnvironmentVariable("EVENT_GRID_ACCESS_KEY");
        services.AddEventGridPublisher(endpoint, accessKey);

        // Register event handlers
        services.AddEventHandlers(typeof(Startup).Assembly);
    }
}
```

## 🎯 Event-Driven Patterns

### 1. Event Notification

**Use Case**: Notify other services about something that happened

```csharp
// Publisher
await _eventPublisher.PublishAsync(new OrderPlacedEvent
{
    Data = new { OrderId = order.Id, Amount = order.Total }
});

// Consumers
// - Email Service: Send order confirmation
// - Inventory Service: Reserve items
// - Analytics Service: Track metrics
```

### 2. Event-Carried State Transfer

**Use Case**: Include all necessary data in the event

```csharp
public class OrderPlacedEvent : DomainEvent<OrderPlacedEventData>
{
    // Include complete order details so consumers don't need to query
}

public class OrderPlacedEventData
{
    public string OrderId { get; set; }
    public string CustomerId { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal Total { get; set; }
    public Address ShippingAddress { get; set; }
}
```

### 3. Event Sourcing (Future)

Store all changes as events:

```csharp
// Coming in v10.5.0
public class OrderAggregate : AggregateRoot
{
    public void PlaceOrder(PlaceOrderCommand cmd)
    {
        RaiseEvent(new OrderPlacedEvent { ... });
    }

    private void Apply(OrderPlacedEvent @event)
    {
        Status = OrderStatus.Placed;
        Total = @event.Data.Total;
    }
}
```

## 🔍 Distributed Tracing

Track events across services:

```csharp
// Service A: Create command
var command = new CreateUserCommand { Email = "test@example.com" };
var correlationId = Guid.NewGuid().ToString();

// Service A: Publish event
var @event = new UserCreatedEvent { Data = userData };
@event.AddCorrelationId(correlationId);
@event.AddCausationId(command.GetHashCode().ToString());

// Service B: Handle event
public class Handler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
    {
        var correlationId = @event.Metadata["correlationId"];
        // Use correlationId for logging, tracing
    }
}
```

## 📊 Platform Comparison

| Feature | AWS EventBridge | Azure Event Grid | RabbitMQ (Future) |
|---------|----------------|------------------|-------------------|
| **CloudEvents** | ✅ | ✅ | ✅ |
| **Batch Publishing** | ✅ (10/batch) | ✅ (100/batch) | ✅ |
| **Cost** | $1/million events | $0.60/million ops | Self-hosted |
| **Max Event Size** | 256 KB | 1 MB | Configurable |
| **Event Replay** | Via Archive | Via Storage | Via Dead Letter |
| **Schema Registry** | ✅ | ✅ | ❌ |
| **Cross-Region** | ✅ | ✅ | Manual |

## 🎯 Best Practices

### 1. Event Naming

Use reverse DNS notation:
```
com.appfactory.user.created
com.appfactory.order.placed
com.appfactory.inventory.reserved
```

### 2. Event Versioning

Include version in event:
```csharp
public class UserCreatedEvent : DomainEvent<UserCreatedEventData>
{
    public UserCreatedEvent()
    {
        Version = 1; // v1 schema
    }
}
```

### 3. Idempotency

Handle duplicate events:
```csharp
public class Handler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
    {
        // Check if already processed
        if (await _processedEvents.ExistsAsync(@event.EventId, ct))
            return;

        // Process event
        await ProcessEvent(@event, ct);

        // Mark as processed
        await _processedEvents.AddAsync(@event.EventId, ct);
    }
}
```

### 4. Error Handling

```csharp
public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
{
    try
    {
        await ProcessEvent(@event, ct);
    }
    catch (TransientException ex)
    {
        // Retry automatically (EventBridge/Event Grid handles this)
        throw;
    }
    catch (PermanentException ex)
    {
        // Log and move to dead letter queue
        _logger.LogError(ex, "Permanent failure processing event");
        // Don't throw - event will be removed from queue
    }
}
```

## 🚀 Next Steps

1. **Implement Event Publishing** - Add event publishing to your command handlers
2. **Create Event Handlers** - Build event handlers for your business logic
3. **Deploy to AWS/Azure** - Use provided base classes for Lambda/Azure Functions
4. **Add Monitoring** - Track event processing metrics
5. **Implement Sagas** - For distributed transactions (coming in v10.5.0)

## 📚 Related Documentation

- [CloudEvents Specification Guide](CLOUDEVENTS_GUIDE.md)
- [Multi-Cloud Events Migration Guide](MULTI_CLOUD_EVENTS_GUIDE.md)
- [AWS EventBridge Package README](src/AppFactory.Framework.EventBus.Aws/README.md)
- [Azure Event Grid Package README](src/AppFactory.Framework.EventBus.Azure/README.md)
- [Sample Applications](samples/EventDriven/README.md)

---

**AppFactory v10.4.0** - Build Event-Driven Microservices, Deploy Anywhere! 🚀
