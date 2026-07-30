# AppFactory v10.4.0 - Release Notes

**Release Date**: 2024  
**Status**: Production Ready

---

## 🎉 What's New

**Event-Driven Architecture Support** - Build decoupled, scalable microservices that communicate through events across AWS and Azure.

---

## 🚀 New Features

### 1. **Multi-Cloud Event Publishing**

Two new packages enable native event publishing:

- **AppFactory.Framework.EventBus.Aws** ⭐ NEW - AWS EventBridge integration
- **AppFactory.Framework.EventBus.Azure** ⭐ NEW - Azure Event Grid integration

### 2. **CloudEvents Standard Support**

Industry-standard CloudEvents v1.0 specification for cross-platform event interoperability.

### 3. **Domain Events**

Strongly-typed domain events with built-in metadata for distributed tracing:

```csharp
var @event = new UserCreatedEvent
{
    Data = new UserCreatedEventData { UserId = "123", Email = "user@example.com" }
};
@event.AddCorrelationId(correlationId);
await _publisher.PublishAsync(@event);
```

### 4. **Event Handlers**

Platform-specific base classes for serverless event processing:

```csharp
// AWS Lambda
public class WelcomeEmailHandler : LambdaEventHandlerBase<UserCreatedEvent> { }

// Azure Functions
public class WelcomeEmailHandler : AzureFunctionEventHandlerBase<UserCreatedEvent> { }
```

### 5. **Batch Publishing**

Efficient batch event publishing for high-throughput scenarios:

```csharp
await _publisher.PublishBatchAsync(events, cancellationToken);
```

### 6. **Event-Driven Patterns**

Support for common event-driven architecture patterns:
- Event Notification
- Event-Carried State Transfer
- Distributed Tracing with Correlation IDs
- Idempotent Event Handling

---

## 📦 Package Updates

**Total Packages**: 21 (2 new EventBus packages added)

### New Packages
- `AppFactory.Framework.EventBus.Aws` v10.4.0
- `AppFactory.Framework.EventBus.Azure` v10.4.0

### Updated Packages
All existing packages updated to v10.4.0 for version consistency.

---

## 🎯 Key Benefits

- ✅ **Decoupled Services** - Services communicate without direct dependencies
- ✅ **Scalable Architecture** - Independent scaling of event producers and consumers
- ✅ **Multi-Cloud Ready** - Same code works on AWS EventBridge and Azure Event Grid
- ✅ **Platform-Agnostic Core** - Share business logic across cloud providers
- ✅ **Production Ready** - Comprehensive error handling, logging, and retry support

---

## 💡 Quick Start

### Installation

```bash
# For AWS EventBridge
dotnet add package AppFactory.Framework.EventBus.Aws --version 10.4.0

# For Azure Event Grid
dotnet add package AppFactory.Framework.EventBus.Azure --version 10.4.0
```

### Basic Usage

**Publish an Event:**
```csharp
public class CreateUserHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IEventPublisher _publisher;
    
    public async Task<CommandResult> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        // 1. Create user
        var user = await _userRepo.AddAsync(new User { Email = cmd.Email }, ct);
        
        // 2. Publish event
        await _publisher.PublishAsync(new UserCreatedEvent 
        { 
            Data = new UserCreatedEventData { UserId = user.Id, Email = user.Email }
        }, ct);
        
        return CommandResult.Success(user.Id);
    }
}
```

**Handle an Event:**
```csharp
// AWS Lambda
public class SendWelcomeEmailHandler : LambdaEventHandlerBase<UserCreatedEvent>
{
    protected override async Task HandleEvent(UserCreatedEvent @event, CancellationToken ct)
    {
        await _emailService.SendWelcomeEmailAsync(@event.Data.Email);
    }
}

// Azure Functions
public class SendWelcomeEmailHandler : AzureFunctionEventHandlerBase<UserCreatedEvent>
{
    protected override async Task HandleEvent(UserCreatedEvent @event, CancellationToken ct)
    {
        await _emailService.SendWelcomeEmailAsync(@event.Data.Email);
    }
}
```

---

## 📊 Use Case Example

**Scenario**: When a user is created, multiple services need to react:

```csharp
// Command Handler publishes event
public class CreateUserHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IEventPublisher _publisher;
    
    public async Task<CommandResult> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        // 1. Create user
        var user = await _userRepo.AddAsync(new User { ... }, ct);
        
        // 2. Publish event
        await _publisher.PublishAsync(new UserCreatedEvent { Data = userData }, ct);
        
        return CommandResult.Success(user.Id);
    }
}

// Multiple services react to the event:
// ✅ Email Service → Send welcome email
// ✅ Analytics Service → Track user signup
// ✅ CRM Service → Create customer profile
// ✅ Notification Service → Send push notification
```

---

## 🔒 Breaking Changes

**None!** ✅

This release is **100% backward compatible** with v10.3.0.

---

## 🔄 Migration from v10.3.0

No code changes required! Simply install the new EventBus packages to add event-driven capabilities.

### Optional: Add EventBus Support

**1. Install Package**
```bash
dotnet add package AppFactory.Framework.EventBus.Aws --version 10.4.0
# or
dotnet add package AppFactory.Framework.EventBus.Azure --version 10.4.0
```

**2. Configure Services**
```csharp
// AWS
services.AddAwsEventBus(options => 
{
    options.EventBusName = "my-event-bus";
});

// Azure
services.AddAzureEventBus(options => 
{
    options.TopicEndpoint = configuration["EventGrid:Endpoint"];
    options.AccessKey = configuration["EventGrid:AccessKey"];
});
```

**3. Inject and Use**
```csharp
public class MyService
{
    private readonly IEventPublisher _publisher;
    
    public MyService(IEventPublisher publisher)
    {
        _publisher = publisher;
    }
}
```

---

## 📚 Documentation

### New Guides
- [Event-Driven Architecture Guide](EVENT_DRIVEN_ARCHITECTURE_GUIDE.md)
- [AWS EventBridge Package README](src/AppFactory.Framework.EventBus.Aws/README.md)
- [Azure Event Grid Package README](src/AppFactory.Framework.EventBus.Azure/README.md)
- [Event-Driven Sample Application](samples/EventDriven/README.md)

### Updated Documentation
- Main [README.md](README.md) - Updated with event-driven examples
- [CHANGELOG.md](CHANGELOG.md) - Complete version history

---

## 🌐 Platform Support

| Platform | Package | Status |
|----------|---------|--------|
| **AWS EventBridge** | AppFactory.Framework.EventBus.Aws | ✅ v10.4.0 |
| **Azure Event Grid** | AppFactory.Framework.EventBus.Azure | ✅ v10.4.0 |
| **AWS Lambda** | AppFactory.Framework.Api.Aws | ✅ v10.3.0 |
| **Azure Functions** | AppFactory.Framework.Api.Azure | ✅ v10.3.0 |
| **ASP.NET Core** | AppFactory.Framework.Api.AspNetCore | ✅ v10.3.0 |
| **AWS DynamoDB** | AppFactory.Framework.DataAccess.DynamoDB | ✅ v10.2.0 |
| **Azure Cosmos DB** | AppFactory.Framework.DataAccess.CosmosDB | ✅ v10.2.0 |

---

## 📈 Platform Comparison

| Feature | AWS EventBridge | Azure Event Grid |
|---------|----------------|------------------|
| **Event Format** | Custom JSON | CloudEvent |
| **Max Event Size** | 256 KB | 1 MB |
| **Batch Size** | 10 events | 100 events |
| **Delivery** | At-least-once | At-least-once |
| **Filtering** | Advanced rules | Basic filters |
| **Pricing** | $1/million events | $0.60/million ops |
| **AppFactory Support** | ✅ v10.4.0 | ✅ v10.4.0 |

---

## 🎯 Event-Driven Benefits

### Decoupling
Services don't need to know about each other. Publishers emit events, consumers react independently.

### Scalability
Each service scales based on its own load. Event processing happens asynchronously.

### Resilience
If one consumer fails, others continue processing. Failed events can be retried or moved to dead-letter queues.

### Flexibility
Add new event consumers without modifying publishers. Change implementations without breaking contracts.

### Audit Trail
Events provide a complete history of what happened in your system, when, and why.

---

## 🔮 What's Next?

### Planned for v10.5.0
- Google Cloud Pub/Sub support
- RabbitMQ EventBus implementation
- Event Sourcing capabilities
- Event replay and audit features
- Saga pattern support for distributed transactions

---

## 📦 Complete Package List (v10.4.0)

| # | Package | Version | Type |
|---|---------|---------|------|
| 1 | AppFactory.Framework.Domain | 10.4.0 | Core |
| 2 | AppFactory.Framework.Application | 10.4.0 | Core |
| 3 | AppFactory.Framework.Shared | 10.4.0 | Core |
| 4 | AppFactory.Framework.DependencyInjection | 10.4.0 | Core |
| 5 | AppFactory.Framework.Logging.Abstractions | 10.4.0 | Logging |
| 6 | AppFactory.Framework.Logging | 10.4.0 | Logging |
| 7 | AppFactory.Framework.Logging.Serilog | 10.4.0 | Logging |
| 8 | AppFactory.Framework.Logging.MicrosoftExtensions | 10.4.0 | Logging |
| 9 | AppFactory.Framework.DataAccess | 10.4.0 | Data |
| 10 | AppFactory.Framework.DataAccess.DynamoDB | 10.4.0 | Data |
| 11 | AppFactory.Framework.DataAccess.CosmosDB | 10.4.0 | Data |
| 12 | AppFactory.Framework.Api | 10.4.0 | API |
| 13 | AppFactory.Framework.Api.Aws | 10.4.0 | API |
| 14 | AppFactory.Framework.Api.Azure | 10.4.0 | API |
| 15 | AppFactory.Framework.Api.AspNetCore | 10.4.0 | API |
| 16 | AppFactory.Framework.Messaging | 10.4.0 | Messaging |
| 17 | AppFactory.Framework.EventBus | 10.4.0 | EventBus |
| 18 | **AppFactory.Framework.EventBus.Aws** | **10.4.0** | **EventBus** ⭐ |
| 19 | **AppFactory.Framework.EventBus.Azure** | **10.4.0** | **EventBus** ⭐ |
| 20 | AppFactory.Framework.TestExtensions | 10.4.0 | Testing |
| 21 | AppFactory.Framework.Infrastructure | 10.4.0 | Infrastructure |

---

## ✅ Testing

All packages include comprehensive unit tests:
- ✅ 54 unit tests passing
- ✅ Event publishing validation
- ✅ Error handling verification
- ✅ Logging confirmation

---

## 🙏 Acknowledgments

Thank you to the .NET community for feedback and support!

---

## 📞 Support

- 🐛 [Report Issues](https://github.com/exiton3/AppFactory/issues)
- 💡 [Request Features](https://github.com/exiton3/AppFactory/issues/new?labels=enhancement)
- 📖 [Documentation](https://github.com/exiton3/AppFactory/wiki)
- ⭐ [Star on GitHub](https://github.com/exiton3/AppFactory)

---

## 🔗 Resources

- **GitHub**: [https://github.com/exiton3/AppFactory](https://github.com/exiton3/AppFactory)
- **NuGet**: [https://www.nuget.org/packages?q=AppFactory.Framework](https://www.nuget.org/packages?q=AppFactory.Framework)
- **License**: MIT
- **Author**: Sergey Kichuk

---

**AppFactory v10.4.0** - Build Event-Driven Microservices, Deploy Anywhere! 🚀

*Release your events, scale your dreams!*
