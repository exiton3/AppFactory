# 🚀 AppFactory v10.4.0 - Complete Multi-Cloud Serverless Framework

## ✅ What You Can Build Now

### 1. Multi-Cloud APIs (v10.3.0)
- ✅ AWS Lambda + API Gateway
- ✅ Azure Functions v4 isolated
- ✅ ASP.NET Core Minimal API (Container Apps)
- **Same code, different deployment targets!**

### 2. Event-Driven Microservices (v10.4.0) **NEW!**
- ✅ AWS EventBridge
- ✅ Azure Event Grid
- ✅ CloudEvents standard
- **Decoupled, scalable, resilient!**

---

## 📦 Complete Package Ecosystem

| Package | Purpose | Deploy To |
|---------|---------|-----------|
| **AppFactory.Framework.Api.Aws** | HTTP APIs | AWS Lambda |
| **AppFactory.Framework.Api.Azure** | HTTP APIs | Azure Functions |
| **AppFactory.Framework.Api.AspNetCore** | HTTP APIs | Container Apps, K8s |
| **AppFactory.Framework.EventBus.Aws** | Events | AWS Lambda |
| **AppFactory.Framework.EventBus.Azure** | Events | Azure Functions |
| **AppFactory.Framework.Application** | CQRS | All Platforms |
| **AppFactory.Framework.Domain** | Domain Models | All Platforms |
| **AppFactory.Framework.DataAccess.DynamoDB** | Data | AWS |
| **AppFactory.Framework.DataAccess.CosmosDB** | Data | Azure |

---

## 🎯 Complete Example: User Registration with Email

### 1. Define Your Domain Event

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
```

### 2. Publish Event from API

```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IRepository<User> _userRepo;
    private readonly IEventPublisher _eventPublisher;

    public async Task<CommandResult> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        // Save to database
        var user = new User { Email = cmd.Email, Name = cmd.Name };
        await _userRepo.AddAsync(user, ct);

        // Publish event
        await _eventPublisher.PublishAsync(new UserCreatedEvent
        {
            Data = new UserCreatedEventData
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name
            }
        }, ct);

        return CommandResult.Success(user.Id);
    }
}
```

### 3. Handle Event (Same Code for AWS/Azure!)

```csharp
public class SendWelcomeEmailHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;

    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
    {
        await _emailService.SendWelcomeEmailAsync(
            @event.Data.Email,
            @event.Data.Name,
            ct);
    }
}
```

### 4a. Deploy to AWS Lambda

```csharp
// API Handler
public class CreateUserLambda : LambdaFunctionHandlerBase<CreateUserCommand, UserDto>
{
    protected override IStartup GetStartup() => new Startup();
}

// Event Handler
public class WelcomeEmailLambda : LambdaEventHandlerBase<UserCreatedEvent>
{
    protected override IStartup GetStartup() => new Startup();
}
```

**serverless.yml:**
```yaml
functions:
  createUser:
    handler: Service::CreateUserLambda::FunctionHandler
    events:
      - http:
          path: users
          method: post

  welcomeEmail:
    handler: Service::WelcomeEmailLambda::FunctionHandler
    events:
      - eventBridge:
          eventBus: my-event-bus
          pattern:
            source: [user-service]
            detail-type: [com.appfactory.user.created]
```

### 4b. Deploy to Azure Functions

```csharp
// API Handler
public class CreateUserFunction : AzureFunctionHandlerBase<CreateUserCommand, UserDto>
{
    [Function("CreateUser")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        FunctionContext context) => await Handle(req, context);
}

// Event Handler
public class WelcomeEmailFunction : AzureFunctionEventHandlerBase<UserCreatedEvent>
{
    [Function("WelcomeEmail")]
    public async Task Run(
        [EventGridTrigger] string eventGridEvent,
        FunctionContext context) => await base.Run(eventGridEvent, context);
}
```

### 4c. Deploy to ASP.NET Core (Container Apps)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add AppFactory services
builder.Services.AddAppFactoryApi(typeof(Program).Assembly);
builder.Services.AddEventGridPublisher(endpoint, accessKey);
builder.Services.AddEventHandlers(typeof(Program).Assembly);

var app = builder.Build();

// Map API endpoint
app.MapCommand<CreateUserCommand, UserDto>("/api/users");

app.Run();

// Event handler runs as background service
public class EventProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Subscribe to Event Grid and process events
    }
}
```

---

## 🎨 Architecture Patterns Supported

### ✅ Clean Architecture
```
Domain (entities, value objects) →
Application (CQRS handlers) →
Infrastructure (AWS/Azure/ASP.NET)
```

### ✅ CQRS
```
Commands (write) ← CommandDispatcher
Queries (read) ← QueryHandler
```

### ✅ Event-Driven Architecture
```
Service A → Event Bus → Services B, C, D
(EventBridge/Event Grid)
```

### ✅ Repository Pattern
```
IRepository<T> →
DynamoDB/CosmosDB Implementation
```

### ✅ Result Pattern
```
CommandResult/Result<T>
(Success/Failure with errors)
```

---

## 📊 Platform Decision Matrix

### Use **AWS Lambda** When:
- ✅ Event-driven workloads
- ✅ Integrating with AWS services (S3, DynamoDB, etc.)
- ✅ Sporadic traffic
- ✅ Cost optimization priority

### Use **Azure Functions** When:
- ✅ Event-driven workloads
- ✅ Integrating with Azure services (Cosmos DB, Storage, etc.)
- ✅ Durable Functions needed
- ✅ Azure ecosystem

### Use **ASP.NET Core** When:
- ✅ High traffic, low latency
- ✅ WebSocket/SignalR needed
- ✅ Long-running connections
- ✅ Full runtime control

---

## 🚀 Quick Start

### 1. Install Packages

```bash
# For AWS Lambda API
dotnet add package AppFactory.Framework.Api.Aws
dotnet add package AppFactory.Framework.EventBus.Aws

# For Azure Functions API
dotnet add package AppFactory.Framework.Api.Azure
dotnet add package AppFactory.Framework.EventBus.Azure

# For ASP.NET Core
dotnet add package AppFactory.Framework.Api.AspNetCore

# Core packages (always needed)
dotnet add package AppFactory.Framework.Application
dotnet add package AppFactory.Framework.Domain
```

### 2. Define Your Models

```csharp
// Command
public class CreateUserCommand : ICommand
{
    [FromBody] public string Email { get; set; }
    [FromBody] public string Name { get; set; }
}

// Event
public class UserCreatedEvent : DomainEvent<UserCreatedEventData>
{
    // Event definition
}

// DTO
public class UserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
}
```

### 3. Implement Handlers

```csharp
// Command Handler (business logic)
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    public async Task<CommandResult> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        // Save user, publish event
    }
}

// Event Handler (business logic)
public class SendWelcomeEmailHandler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
    {
        // Send email
    }
}
```

### 4. Deploy!

Choose your platform and deploy - **same code works everywhere!**

---

## 📚 Documentation

### Core Guides
- [Multi-Cloud API Migration Guide](MULTI_CLOUD_API_MIGRATION_GUIDE.md)
- [Event-Driven Architecture Guide](EVENT_DRIVEN_ARCHITECTURE_GUIDE.md)
- [Multi-Cloud API Quick Reference](MULTI_CLOUD_API_QUICK_REFERENCE.md)

### Package READMEs
- [AWS Lambda API](src/AppFactory.Framework.Api.Aws/README.md)
- [Azure Functions API](src/AppFactory.Framework.Api.Azure/README.md)
- [ASP.NET Core API](src/AppFactory.Framework.Api.AspNetCore/README.md)

### Samples
- [API Samples](samples/README.md)
- [Event-Driven Samples](samples/EventDriven/README.md)

---

## 🎯 What Makes AppFactory Special?

### 1. True Multi-Cloud ☁️
Write once, deploy to AWS, Azure, or on-premises without code changes.

### 2. Clean Architecture 🏗️
Domain and application logic completely platform-agnostic.

### 3. CQRS Built-In 🎯
Command and query separation with type-safe handlers.

### 4. Event-Driven by Default 📡
CloudEvents-compliant events work across all platforms.

### 5. Production Ready ✅
Based on real-world best practices and battle-tested patterns.

### 6. Zero Vendor Lock-In 🔓
Switch clouds or deployment models anytime.

### 7. Developer Joy 😊
Intuitive APIs, comprehensive docs, working samples.

---

## 🎉 Complete Feature Matrix

| Feature | v10.1.0 | v10.2.0 | v10.3.0 | v10.4.0 |
|---------|---------|---------|---------|---------|
| CQRS | ✅ | ✅ | ✅ | ✅ |
| Assembly Scanning | ✅ | ✅ | ✅ | ✅ |
| Logging (Serilog/MEL) | | ✅ | ✅ | ✅ |
| AWS Lambda API | | | ✅ | ✅ |
| Azure Functions API | | | ✅ | ✅ |
| ASP.NET Core API | | | ✅ | ✅ |
| AWS EventBridge | | | | ✅ |
| Azure Event Grid | | | | ✅ |
| CloudEvents | | | | ✅ |
| Distributed Tracing | | | | ✅ |

---

**AppFactory v10.4.0** - The Complete Multi-Cloud Serverless Framework for .NET! 🚀

**Build Once. Deploy Anywhere. Scale Everywhere.** ☁️🌍🚀
