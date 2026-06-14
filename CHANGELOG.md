# Changelog

All notable changes to the AppFactory Framework will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [10.5.0] - 2026-06-13

### 🚀 Unified Multi-Cloud Messaging Architecture

Major update introducing a unified, platform-agnostic messaging architecture with support for AWS SQS, Azure Service Bus, and Azure Storage Queues.

### Added

#### Unified Messaging Abstractions (`AppFactory.Framework.Messaging.Core`)
- **IMessageHandler<TMessage>** - Platform-agnostic message handler interface (single source of truth)
- **Message** class - Unified message structure with:
  - `Properties` dictionary for platform-specific metadata
  - `EnqueuedTimeUtc` for message timing
  - `DeliveryCount` for retry tracking
- **IMessageHandler<TMessage, TContext>** - Extended handler with message context support
- **ServiceCollectionExtensions** - Easy DI registration
  - `AddMessageHandler<THandler, TMessage>()` - Register single handler
  - `AddMessageHandlers(assemblies)` - Auto-register handlers from assemblies

#### AWS SQS Integration (Enhanced `AppFactory.Framework.Messaging.Aws`)
- **SqsMessageHandlerBase<TMessage>** - AWS Lambda SQS handler with Publisher-Subscriber pattern
  - Platform-agnostic `IMessageHandler<TMessage>` support
  - Automatic SQS → Message mapping with full metadata
  - Batch failure handling with partial retries
  - Performance logging and cancellation support
- **SqsMessagePublisher** - Publish messages to SQS queues
  - Fixed batch publishing to use `List<PublishResult>`
  - Updated return types to match interface contracts

#### Azure Service Bus Integration (`AppFactory.Framework.Messaging.Azure`) **NEW**
- **ServiceBusFunctionHandlerBase<TMessage>** - Azure Functions handler for Service Bus
  - Queue message processing
  - Topic subscription processing
  - Batch processing support
  - Same DI and processor pattern as AWS
- **QueueStorageFunctionHandlerBase<TMessage>** - Azure Functions handler for Storage Queues
  - QueueMessage and string message support
  - Batch processing support
- **ServiceBusMessagePublisher** - Publish to Service Bus queues/topics
- **QueueStorageMessagePublisher** - Publish to Storage Queues
- Namespace alias to avoid collision: `using AzureServiceBus = Azure.Messaging.ServiceBus;`

### Changed
- **Removed** `IMessageProcessor<TMessage>` (duplicate interface)
- `ILambdaMessageProcessor<T>` remains for backward compatibility (legacy)
- All new handlers use `IMessageHandler<TMessage>` from `Messaging.Core`
- Message metadata access pattern unified:
  - `message.Properties["key"]` instead of `message.Attributes["key"]`
  - `message.EnqueuedTimeUtc` instead of `message.Timestamp`
  - `message.DeliveryCount` for retry count across all platforms

### Fixed
- AWS `SqsMessagePublisher` batch publishing now returns `List<PublishResult>`
- AWS `LambdaMessageHandlerBase` fixed to use `Message` class with settable properties
- Implemented missing `IMessageContext` properties in `SqsMessageContext`
- Package version conflicts in Azure packages (Microsoft.Extensions.* → 10.0.8)
- NuGet pack error NU5033: Removed duplicate `PackageLicenseExpression` from Messaging.Azure and Messaging.Aws

### Security
- Updated AWS SDK packages to latest versions:
  - AWSSDK.EventBridge: 4.0.5.33 → 4.0.6.1
  - AWSSDK.DynamoDBv2: 4.0.18.5 → 4.0.18.6
  - AWSSDK.SQS: 4.0.2.32 → 4.0.2.33
- Updated Azure packages with security patches:
  - Azure.Messaging.ServiceBus: 7.17.2 → 7.20.1
  - Azure.Storage.Queues: 12.17.0 → 12.26.0
  - Microsoft.Azure.Functions.Worker: 2.0.0 → 2.52.0 (52 minor versions!)
  - Azure.Messaging.EventGrid: 4.28.0 → 4.31.0
- Updated Microsoft.Extensions packages: 9.0.0 → 10.0.8
- Updated test frameworks: xunit 2.9.2 → 2.9.3, coverlet 6.0.2 → 6.0.4

### Platform Support
- ✅ AWS Lambda + SQS
- ✅ Azure Functions + Service Bus Queue
- ✅ Azure Functions + Service Bus Topic
- ✅ Azure Functions + Storage Queue
- All use the same `IMessageHandler<TMessage>` interface!

### Example Usage

#### Platform-Agnostic Handler (Works on AWS & Azure!)
```csharp
public class OrderHandler : IMessageHandler<OrderMessage>
{
    public async Task HandleAsync(OrderMessage message, CancellationToken ct)
    {
        // Access metadata (same pattern on all platforms)
        var source = message.Properties.GetValueOrDefault("Source");
        var deliveryCount = message.DeliveryCount;
        var enqueuedTime = message.EnqueuedTimeUtc;

        // Business logic
        var data = JsonSerializer.Deserialize<OrderData>(message.Body);
        await _orderService.ProcessAsync(data, ct);
    }
}

// Register once, works everywhere
services.AddScoped<IMessageHandler<OrderMessage>, OrderHandler>();
```

#### AWS Lambda Function
```csharp
public class OrderFunction : SqsMessageHandlerBase<OrderMessage>
{
    public OrderFunction() : base(new Startup()) { }
    protected override IStartup GetStartup() => new Startup();
}
```

#### Azure Service Bus Function
```csharp
public class OrderFunction : ServiceBusFunctionHandlerBase<OrderMessage>
{
    [Function("ProcessOrder")]
    public async Task Run(
        [ServiceBusTrigger("queue", Connection = "ServiceBus")] 
        ServiceBusReceivedMessage msg, FunctionContext ctx)
        => await Handle(msg, ctx);
}
```

### Migration Notes
- Existing `ILambdaMessageProcessor<T>` code continues to work (no breaking changes)
- For new code, use `IMessageHandler<TMessage>` from `Messaging.Core`
- Update message property access: `Attributes` → `Properties`

## [10.4.0] - 2026-05-28

### 🎯 Event-Driven Architecture - Multi-Cloud Event Support!

AppFactory now provides comprehensive event-driven architecture support across AWS EventBridge, Azure Event Grid, and on-premises messaging systems.

### Added

#### Core Event Abstractions (`AppFactory.Framework.EventBus`)
- **IEvent** - Platform-agnostic event interface
- **IEventPublisher** - Unified event publishing across all platforms
- **IEventHandler<TEvent>** - Platform-agnostic event handler interface
- **CloudEvent** - CloudEvents 1.0 specification support
- **DomainEvent<TData>** - Strongly-typed domain events with metadata support
  - Correlation ID tracking
  - Causation ID tracking
  - User context tracking

#### AWS EventBridge Integration (Enhanced `AppFactory.Framework.EventBus.Aws`)
- **EventBridgePublisher** - Publish events to AWS EventBridge
- **LambdaEventHandlerBase<TEvent>** - Process EventBridge events in Lambda
- **ServiceCollectionExtensions** - Easy DI setup
  - `AddEventBridgePublisher()` - Register EventBridge publisher
  - `AddEventHandlers()` - Auto-register event handlers from assembly
- Batch publishing support (up to 10 events per batch)
- Full CloudEvents compatibility

#### Azure Event Grid Integration (`AppFactory.Framework.EventBus.Azure`) **NEW**
- **EventGridPublisher** - Publish CloudEvents to Azure Event Grid
- **AzureFunctionEventHandlerBase<TEvent>** - Process Event Grid events in Azure Functions
- **ServiceCollectionExtensions** - Easy DI setup
  - `AddEventGridPublisher()` - Register Event Grid publisher
  - `AddEventHandlers()` - Auto-register event handlers from assembly
- Native CloudEvents support
- Batch publishing support

### Changed
- Enhanced existing `IEventBus` to work with new `IEventPublisher` abstraction
- `IntegrationEvent` now implements `IEvent` for backward compatibility
- Improved event serialization with CloudEvents format

### Event-Driven Patterns Supported
- ✅ **Event Publishing** - Publish domain events to EventBridge or Event Grid
- ✅ **Event Handling** - Subscribe to events and process them in Lambda/Azure Functions
- ✅ **CloudEvents Standard** - Industry-standard event format
- ✅ **Batch Publishing** - Publish multiple events efficiently
- ✅ **Distributed Tracing** - Correlation IDs and causation IDs
- ✅ **Cross-Service Communication** - Decoupled microservices architecture

### Sample Applications
- ✅ `samples/EventDriven.Aws.UserService` - Complete AWS EventBridge example
  - User creation with event publishing
  - Welcome email event handler
  - EventBridge event bus configuration

### Documentation
- [Event-Driven Architecture Guide](EVENT_DRIVEN_ARCHITECTURE_GUIDE.md)
- [CloudEvents Specification Guide](CLOUDEVENTS_GUIDE.md)
- [Multi-Cloud Events Migration Guide](MULTI_CLOUD_EVENTS_GUIDE.md)
- [AWS EventBridge Package README](src/AppFactory.Framework.EventBus.Aws/README.md)
- [Azure Event Grid Package README](src/AppFactory.Framework.EventBus.Azure/README.md)

### Example Usage

#### Publishing Events
```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IEventPublisher _eventPublisher;

    public async Task<CommandResult> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        var user = new User { Email = cmd.Email };
        await _userRepo.AddAsync(user, ct);

        // Publish event (works on AWS, Azure, or on-prem!)
        await _eventPublisher.PublishAsync(new UserCreatedEvent
        {
            EventType = "com.appfactory.user.created",
            Source = "user-service",
            Data = new { UserId = user.Id, Email = user.Email }
        }, ct);

        return CommandResult.Success(user.Id);
    }
}
```

#### Handling Events
```csharp
public class SendWelcomeEmailHandler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
    {
        await _emailService.SendWelcomeEmailAsync(@event.Data.Email, ct);
    }
}

// AWS Lambda
public class UserCreatedLambda : LambdaEventHandlerBase<UserCreatedEvent>
{
    protected override IStartup GetStartup() => new Startup();
}

// Azure Functions
public class UserCreatedFunction : AzureFunctionEventHandlerBase<UserCreatedEvent>
{
    protected override IStartup GetStartup() => new Startup();
}
```

## [10.3.0] - 2024-12-19

### 🚀 Multi-Cloud API Support - Build Once, Deploy Anywhere!

AppFactory now supports building serverless and containerized APIs across **AWS Lambda**, **Azure Functions**, and **ASP.NET Core** with a unified, platform-agnostic architecture.

### Added

#### Core Abstractions (`AppFactory.Framework.Api`)
- **IHttpRequestContext** - Platform-agnostic HTTP request abstraction
- **IHttpResponseBuilder** - Platform-agnostic HTTP response builder
- **IFunctionProcessor<TRequest, TResponse>** - Unified processor interface (replaces `ILambdaProcessor`)
- **HttpMethod** - Platform-agnostic HTTP method enum
- **HttpStatusCode** - Platform-agnostic HTTP status codes
- **FunctionHandlerCore<TRequest, TResponse>** - Shared request handling logic across all platforms

#### AWS Lambda Package (`AppFactory.Framework.Api.Aws`)
- **LambdaFunctionHandlerBase<TRequest, TResponse>** - AWS Lambda function base class
- **ApiGatewayRequestContext** - API Gateway request adapter implementing `IHttpRequestContext`
- **ApiGatewayResponseBuilder** - API Gateway response builder implementing `IHttpResponseBuilder`
- Full backward compatibility with existing AWS Lambda code
- Support for API Gateway proxy integration

#### Azure Functions Package (`AppFactory.Framework.Api.Azure`)
- **AzureFunctionHandlerBase<TRequest, TResponse>** - Azure Functions v4 isolated worker base class
- **HttpRequestDataContext** - Azure Functions request adapter implementing `IHttpRequestContext`
- **HttpResponseDataBuilder** - Azure Functions response builder implementing `IHttpResponseBuilder`
- Support for Azure Functions v4 isolated worker model
- Full compatibility with latest Azure Functions runtime

#### ASP.NET Core Package (`AppFactory.Framework.Api.AspNetCore`)
- **EndpointRouteBuilderExtensions** - Fluent API for mapping CQRS endpoints
  - `MapCqrsEndpoint<TRequest, TResponse>()` - Map generic CQRS endpoint
  - `MapCommand<TCommand, TResponse>()` - Map command to POST endpoint
  - `MapQuery<TQuery, TResponse>()` - Map query to GET endpoint
- **ServiceCollectionExtensions** - Service registration extensions
  - `AddAppFactoryApi()` - Register core API services
  - `AddAppFactoryApiWithCqrs()` - Register API services with CQRS
- **AspNetCoreRequestContext** - ASP.NET Core request adapter
- **AspNetCoreResponseBuilder** - ASP.NET Core response builder
- **ExceptionHandlingMiddleware** - Global exception handling with problem details
- **RequestLoggingMiddleware** - Performance tracking and request logging
- Perfect for Azure Container Apps, Kubernetes, and traditional hosting

### Changed

- **ILambdaProcessor** marked as `[Obsolete]` - Use `IFunctionProcessor` for platform-agnostic code
- Existing AWS Lambda code continues to work with zero breaking changes
- Request parsing and response building now platform-agnostic
- Improved error handling with consistent problem details format across all platforms

### Migration Benefits

- ✅ **Zero Breaking Changes** - Existing AWS Lambda code works as-is
- ✅ **Write Once, Deploy Anywhere** - Same business logic across AWS, Azure, and ASP.NET Core
- ✅ **Clean Architecture** - Platform-agnostic domain and application layers
- ✅ **Developer Experience** - Consistent API across all platforms
- ✅ **Production Ready** - Based on Azure and AWS best practices

### Deployment Options

Your applications can now be deployed to:
- **AWS Lambda + API Gateway** - Event-driven serverless on AWS
- **Azure Functions (v4 isolated)** - Event-driven serverless on Azure
- **Azure Container Apps** - Containerized, always-on APIs
- **Kubernetes (AKS/EKS)** - Full orchestration control
- **Traditional VMs/App Services** - Classic hosting

### Documentation

- [Multi-Cloud API Migration Guide](MULTI_CLOUD_API_MIGRATION_GUIDE.md)
- [Multi-Cloud API Implementation Summary](MULTI_CLOUD_API_IMPLEMENTATION_SUMMARY.md)
- [Multi-Cloud API Quick Reference](MULTI_CLOUD_API_QUICK_REFERENCE.md)
- [AWS Lambda Package README](src/AppFactory.Framework.Api.Aws/README.md)
- [Azure Functions Package README](src/AppFactory.Framework.Api.Azure/README.md)
- [ASP.NET Core Package README](src/AppFactory.Framework.Api.AspNetCore/README.md)

### Example Usage

#### Shared Processor (Works Everywhere!)
```csharp
public class CreateUserProcessor : IFunctionProcessor<CreateUserCommand, UserDto>
{
    public async Task<Result<UserDto>> Process(CreateUserCommand request, CancellationToken ct)
    {
        // Same implementation works across AWS, Azure, and ASP.NET Core!
    }
}
```

#### AWS Lambda
```csharp
public class CreateUserLambda : LambdaFunctionHandlerBase<CreateUserCommand, UserDto>
{
    protected override IStartup GetStartup() => new Startup();
}
```

#### Azure Functions
```csharp
public class CreateUserFunction : AzureFunctionHandlerBase<CreateUserCommand, UserDto>
{
    [Function("CreateUser")]
    public async Task<HttpResponseData> Run([HttpTrigger] HttpRequestData req, FunctionContext ctx)
        => await Handle(req, ctx);
}
```

#### ASP.NET Core
```csharp
app.MapCommand<CreateUserCommand, UserDto>("/api/users");
```

## [10.2.0] - 2024-12-15

### Added
- **Microsoft.Extensions.Logging Implementation** (`AppFactory.Framework.Logging.MicrosoftExtensions`)
  - Adapter for standard .NET logging
  - `AddMicrosoftExtensionsLogging()` extension methods
  - Perfect for ASP.NET Core applications
  - Supports all MEL providers (Console, Debug, ApplicationInsights, etc.)
  - Performance tracking with structured logging

- **Logging Abstractions** (`AppFactory.Framework.Logging.Abstractions`)
  - Core logging interfaces (`ILogger`, `ILoggerFactory`)
  - No dependencies on specific logging frameworks
  - Allows choosing logging implementation

- **Serilog Logging Implementation** (`AppFactory.Framework.Logging.Serilog`)
  - Moved Serilog implementation to dedicated package
  - `AddSerilogLogging()` extension methods
  - Support for custom Serilog configuration
  - Performance tracking with structured logging

### Changed
- **Logging Package Split** - `AppFactory.Framework.Logging` split into:
  - `AppFactory.Framework.Logging.Abstractions` - Interfaces only
  - `AppFactory.Framework.Logging.Serilog` - Serilog implementation
  - `AppFactory.Framework.Logging.MicrosoftExtensions` - MEL implementation
  - Original package maintained for backward compatibility

## [10.1.0] - 2024-05-28

### Added
- **Assembly Scanning Framework** (`AppFactory.Framework.DependencyInjection`)
  - Fluent API for scanning assemblies and automatically registering types
  - Support for filtering by interface, namespace, and name patterns
  - Multiple registration strategies (Append, Skip, Replace)
  - Service lifetime selection (Singleton, Scoped, Transient)
  - Similar functionality to [Scrutor](https://github.com/khellang/Scrutor)

- **Application Layer** (`AppFactory.Framework.Application`)
  - `AddCqrs()` - Now uses assembly scanning internally
  - `AddCommandHandlers()` - Scan and register command handlers
  - `AddQueryHandlers()` - Scan and register query handlers

- **Data Access - CosmosDB** (`AppFactory.Framework.DataAccess.CosmosDB`)
  - `RegisterModelConfigs()` - Scan and register all model configurations
  - `RegisterRepositories()` - Scan and register all repositories
  - `RegisterCosmosDbDataAccess()` - All-in-one registration method

- **Data Access - DynamoDB** (`AppFactory.Framework.DataAccess.DynamoDB`)
  - `RegisterModelConfigs()` - Scan and register all model configurations
  - `RegisterRepositories()` - Scan and register all repositories
  - `RegisterDynamoDbDataAccess()` - All-in-one registration method

- **Data Access - Legacy** (`AppFactory.Framework.DataAccess`)
  - `RegisterModelConfigs()` - Scan and register all model configurations
  - `RegisterRepositories()` - Scan and register all repositories
  - `RegisterDynamoDbDataAccess()` - All-in-one registration method

### Changed
- Enhanced XML documentation across all dependency injection extension methods
- Improved code consistency between CosmosDB and DynamoDB implementations
- Updated CQRS registration to leverage assembly scanning

### Dependencies
- Added project reference to `AppFactory.Framework.DependencyInjection` in:
  - `AppFactory.Framework.Application`
  - `AppFactory.Framework.DataAccess`
  - `AppFactory.Framework.DataAccess.CosmosDB`
  - `AppFactory.Framework.DataAccess.DynamoDB`

### Migration Guide

#### Before (Manual Registration)
```csharp
services.AddScoped<IRepository<User>, UserRepository>();
services.AddScoped<IRepository<Product>, ProductRepository>();
services.AddSingleton<IModelConfig<User>, UserModelConfig>();
services.AddSingleton<IModelConfig<Product>, ProductModelConfig>();
```

#### After (Assembly Scanning)
```csharp
// Option 1: All-in-one
services.RegisterCosmosDbDataAccess(typeof(UserRepository).Assembly);

// Option 2: Explicit scanning
services.Scan(scan => scan
    .FromAssembliesOf(typeof(UserRepository))
    .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)), publicOnly: false)
        .AsImplementedInterfaces()
        .WithScopedLifetime());
```

## [10.0.6] - 2024-XX-XX

### Initial Release
- Core framework components
- Domain layer with repositories and commands
- Application layer with CQRS support
- Data access for DynamoDB and CosmosDB
- Logging infrastructure with Serilog
- API framework for AWS Lambda
- Event bus integration
- Messaging handlers
- Test extensions

---

## Version Numbering

AppFactory Framework uses [Semantic Versioning](https://semver.org/):

- **MAJOR** version (X.0.0) - Incompatible API changes
- **MINOR** version (0.X.0) - New functionality in a backward compatible manner
- **PATCH** version (0.0.X) - Backward compatible bug fixes

All packages in the framework are versioned together to ensure compatibility.
