# AppFactory

[![GitHub](https://img.shields.io/github/license/exiton3/AppFactory)](https://github.com/exiton3/AppFactory/blob/master/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/AppFactory.Framework.Domain.svg)](https://www.nuget.org/packages/AppFactory.Framework.Domain/)

A simple and lightweight application framework for building CQRS and DDD projects in .NET 10.

**Build serverless, event-driven distributed applications on AWS without the complexity.** AppFactory combines **Event-Driven Architecture (EDA)**, **Serverless Computing**, and **Reactive Microservices** to create highly scalable and resilient systems that respond to changes in real-time without the overhead of managing infrastructure.

## 🚀 Overview

**AppFactory** is a streamlined framework designed to accelerate the development of applications using **Command Query Responsibility Segregation (CQRS)** and **Domain-Driven Design (DDD)** patterns. It provides essential building blocks without the overhead of complex enterprise frameworks, making it ideal for modern .NET applications.

**Perfect for AWS Serverless:** AppFactory is specifically designed for building serverless, event-driven distributed applications on AWS. With native support for **AWS Lambda**, **EventBridge**, **SQS**, and **DynamoDB**, you can focus on writing business logic while the framework handles infrastructure concerns, deployment patterns, and AWS service integrations.

## 🏗️ Core Architectural Components

AppFactory combines powerful architectural patterns and design principles to build modern distributed systems:

### 🎨 Clean Architecture & DDD
- **Domain-Centric Design** - Business logic remains independent of infrastructure concerns
- **Separation of Concerns** - Clear boundaries between domain, application, and infrastructure layers
- **CQRS Pattern** - Command and query segregation for optimized read/write operations
- **Rich Domain Models** - Entities and value objects encapsulate business rules and behavior

### 1️⃣ Event-Driven Architecture (EDA)
- **Decoupled Services** - Services communicate through events (state changes like user registration, order placement, or file upload)
- **Event Broadcasting** - EventBridge publishes events to multiple subscribers without tight coupling
- **Asynchronous Communication** - Services react to events independently, improving resilience and scalability
- **Real-Time Responsiveness** - System responds immediately to state changes across distributed components

### 2️⃣ Serverless Computing
- **Zero Infrastructure Management** - Focus on code, not servers
- **Automatic Scaling** - Handles traffic spikes without manual intervention
- **Pay-per-Use Model** - Cost-effective with no idle resource charges
- **Event-Driven Execution** - Functions triggered by API calls, events, and messages

### 3️⃣ Reactive Microservices
- **Responsive** - Services react quickly to requests and events with non-blocking I/O
- **Resilient** - Failures are isolated; services handle errors gracefully and recover automatically
- **Elastic** - Services scale up or down based on demand without manual intervention
- **Message-Driven** - Asynchronous message passing ensures loose coupling and location transparency

**Together, these patterns enable:**
- ✅ Highly scalable systems that handle traffic spikes effortlessly
- ✅ Resilient architecture that degrades gracefully under failure
- ✅ Real-time responsiveness to business events
- ✅ Cost-effective infrastructure with pay-per-use pricing
- ✅ Independent deployment and evolution of services

## ✨ Features

- **🎯 CQRS Pattern Implementation** - Clean separation of commands and queries with dedicated handlers
- **📦 Domain-Driven Design Building Blocks** - Entities, Value Objects, and Domain Events
- **✅ Result Pattern** - Type-safe command results with comprehensive error handling
- **☁️ Serverless-First Architecture** - Build highly scalable distributed solutions without infrastructure overhead using AWS Lambda
- **🔄 Event-Driven Architecture (EDA)** - Decouple services using events as triggers for communication, enabling real-time reactive systems
- **🎯 Reactive Microservices** - Create responsive, resilient, and elastic services with asynchronous message passing and non-blocking I/O
- **💾 DynamoDB Integration** - Complete data access layer with repository pattern for NoSQL persistence
- **☁️ CosmosDB Support** - Azure Cosmos DB data access with the same fluent repository pattern
- **🚀 API Gateway Support** - Lambda function handlers with automatic request parsing and response building
- **⚡ Lightweight & Fast** - Minimal dependencies and optimized performance
- **🔧 Flexible & Extensible** - Easy to customize and extend to fit your needs
- **🆕 .NET 10 Ready** - Built for the latest .NET framework

## 📦 Installation

Install the AppFactory NuGet packages:

**Core Domain Package:**
```bash
dotnet add package AppFactory.Framework.Domain
```

**Application Layer (CQRS):**
```bash
dotnet add package AppFactory.Framework.Application
```

**For AWS Lambda API Development:**
```bash
dotnet add package AppFactory.Framework.Api
```

**For EventBridge Integration:**
```bash
dotnet add package AppFactory.Framework.EventBus.Aws
```

**For SQS Messaging:**
```bash
dotnet add package AppFactory.Framework.Messaging
```

**For DynamoDB Data Access:**
```bash
dotnet add package AppFactory.Framework.DataAccess.DynamoDB
```

**For Azure Cosmos DB Data Access:**
```bash
dotnet add package AppFactory.Framework.DataAccess.CosmosDB
```

Or via Package Manager Console:

```powershell
Install-Package AppFactory.Framework.Domain
Install-Package AppFactory.Framework.Application
Install-Package AppFactory.Framework.Api
Install-Package AppFactory.Framework.EventBus.Aws
Install-Package AppFactory.Framework.Messaging
Install-Package AppFactory.Framework.DataAccess.DynamoDB
Install-Package AppFactory.Framework.DataAccess.CosmosDB
```

## 🏗️ Core Components

### Commands

Commands represent operations that change the system state. They return a `CommandResult` indicating success or failure.

**Define a Command:**

```csharp
using AppFactory.Framework.Application.Commands;

public class CreateUserCommand : ICommand
{
    public string Email { get; set; }
    public string Name { get; set; }
}
```

**Implement a Command Handler:**

```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    public async Task<CommandResult> Handle(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        // Validate command
        if (string.IsNullOrEmpty(command.Email))
        {
            return CommandResult.ErrorResult("INVALID_EMAIL", "Email is required");
        }

        // Process command
        var user = new User
        {
            Email = command.Email,
            Name = command.Name
        };

        // Save to database...

        return CommandResult.Success(user.Id);
    }
}
```

### Queries

Queries retrieve data without modifying system state.

**Define a Query:**

```csharp
using AppFactory.Framework.Application.Queries;

public class GetUserByIdQuery : IQueryRequest
{
    public string UserId { get; set; }
}

public class UserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}
```

**Implement a Query Handler:**

```csharp
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken = default)
    {
        // Retrieve and return data from database...
        return new UserDto 
        { 
            Id = request.UserId, 
            Email = "user@example.com",
            Name = "John Doe" 
        };
    }
}
```

### Dependency Registration

Register CQRS components in your application's dependency injection container:

**Register All CQRS Components:**

```csharp
using AppFactory.Framework.Application;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Automatically registers CommandDispatcher and scans assemblies for all handlers
services.AddCqrs(typeof(CreateUserCommandHandler).Assembly);

// Or scan multiple assemblies
services.AddCqrs(
    typeof(CreateUserCommandHandler).Assembly,
    typeof(GetUserByIdQueryHandler).Assembly
);
```

**Register Only Command Handlers:**

```csharp
// Registers CommandDispatcher + Command Handlers only
services.AddCommandHandlers(typeof(CreateUserCommandHandler).Assembly);
```

**Register Only Query Handlers:**

```csharp
// Registers Query Handlers only
services.AddQueryHandlers(typeof(GetUserByIdQueryHandler).Assembly);
```

**Register Specific Handlers Manually:**

```csharp
// Register individual handlers
services.AddCommandHandler<CreateUserCommandHandler>();
services.AddQueryHandler<GetUserByIdQueryHandler>();
```

**Use in Your Application:**

```csharp
public class UserService
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryHandler<GetUserByIdQuery, UserDto> _queryHandler;

    public UserService(
        ICommandDispatcher commandDispatcher,
        IQueryHandler<GetUserByIdQuery, UserDto> queryHandler)
    {
        _commandDispatcher = commandDispatcher;
        _queryHandler = queryHandler;
    }

    public async Task<string> CreateUserAsync(string email, string name)
    {
        var command = new CreateUserCommand { Email = email, Name = name };
        var result = await _commandDispatcher.Dispatch(command);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Errors.First().Message);

        return result.Id;
    }

    public async Task<UserDto> GetUserAsync(string userId)
    {
        var query = new GetUserByIdQuery { UserId = userId };
        return await _queryHandler.Handle(query);
    }
}
```

> **Note:** For detailed CQRS patterns, handler implementation, and best practices, see [AppFactory.Framework.Application README](src/AppFactory.Framework.Application/README.md)

### Entities

Domain entities with built-in identity equality.

**Simple Entity (String ID):**

```csharp
using AppFactory.Framework.Domain.Entities;

public class User : Entity
{
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Entity with Custom ID Type:**

```csharp
public class Order : EntityWithTypedId<Guid>
{
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItem> Items { get; set; }
}
```

Entities automatically implement proper equality comparison based on their ID.

### Command Results

Strongly-typed results with comprehensive error handling and failure categorization.

**Success Scenarios:**

```csharp
using AppFactory.Framework.Domain.Commands;

// Simple success
var success = CommandResult.Success();

// Success with entity ID
var successWithId = CommandResult.Success("entity-id-123");

// Check result
if (result.IsSuccess)
{
    Console.WriteLine($"Created entity with ID: {result.Id}");
}
```

**Error Scenarios:**

```csharp
using AppFactory.Framework.Domain.ServiceResult;

// Single validation error
var validationError = CommandResult.ErrorResult("VALIDATION_ERROR", "Invalid input");

// Domain error with specific failure reason
var domainError = CommandResult.ErrorResult(
    new Error("BUSINESS_RULE", "Cannot process order: insufficient stock"),
    FailureReason.DomainError
);

// Multiple errors
var multipleErrors = CommandResult.ErrorResult(
    new List<Error>
    {
        new Error("EMAIL_REQUIRED", "Email is required"),
        new Error("NAME_INVALID", "Name must be at least 2 characters")
    },
    FailureReason.Validation
);

// Handle errors
if (result.IsFailure)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.Code}: {error.Message}");
    }
}
```

**Failure Reasons:**

Built-in failure categorization for better error handling:

| Failure Reason | Description |
|----------------|-------------|
| `None` | No failure (success state) |
| `Validation` | Input validation errors |
| `DomainError` | Business rule violations |
| `ExternalSystem` | External system failures (API calls, third-party services) |

## ☁️ AWS Serverless Components

AppFactory provides first-class support for building serverless, event-driven applications on AWS.

### Lambda Function Handlers

Create API endpoints with AWS Lambda and API Gateway with minimal boilerplate.

**Define a Lambda Function Handler:**

```csharp
using AppFactory.Framework.Api.LambdaFunctionHandlers;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

public class CreateUserLambda : LambdaFunctionHandlerBase<CreateUserCommand, UserDto>
{
    protected override IStartup GetStartup()
    {
        return new Startup(); // Your DI configuration
    }

    // Framework handles request parsing, validation, and response building
    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request, 
        ILambdaContext context)
    {
        return await Handle(request, context);
    }
}
```

The framework automatically:
- Parses API Gateway requests (path, query, body parameters)
- Validates input
- Executes your command/query handlers
- Builds proper HTTP responses with error handling

### Event-Driven Messaging with EventBridge

Publish and subscribe to domain events using AWS EventBridge.

**Publish Events:**

```csharp
using AppFactory.Framework.EventBus.EventBus;
using AppFactory.Framework.EventBus.EventBus.Events;

public class UserCreatedEvent : IntegrationEvent
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public override string Source => "user-service";
}

// In your command handler
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IEventBus _eventBus;

    public async Task<CommandResult> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Create user...

        // Publish event to EventBridge
        _eventBus.Publish(new UserCreatedEvent 
        { 
            UserId = user.Id, 
            Email = user.Email 
        });

        return CommandResult.Success(user.Id);
    }
}
```

**Subscribe to Events:**

```csharp
using AppFactory.Framework.EventBus.EventBus;

public class UserCreatedEventHandler : IIntegrationEventHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent @event)
    {
        // Handle the event (e.g., send welcome email, update analytics)
        Console.WriteLine($"User created: {@event.UserId}");
    }
}

// Register subscription
_eventBus.Subscribe<UserCreatedEvent, UserCreatedEventHandler>();
```

### SQS Message Publishing

Send messages to SQS queues for asynchronous processing.

```csharp
using AppFactory.Framework.Messaging.Publishers;

public class OrderProcessingMessage : MessageBase
{
    public OrderProcessingMessage(string orderId)
    {
        Body = JsonSerializer.Serialize(new { OrderId = orderId });
        Attributes = new Dictionary<string, string>
        {
            { "MessageType", "OrderProcessing" }
        };
    }
}

// Publish to SQS
public class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand>
{
    private readonly IMessagePublisher _messagePublisher;

    public async Task<CommandResult> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        // Create order...

        // Send to SQS for async processing
        await _messagePublisher.Publish(
            new OrderProcessingMessage(order.Id), 
            cancellationToken
        );

        return CommandResult.Success(order.Id);
    }
}
```

### DynamoDB Data Access

Work with AWS DynamoDB using a clean repository pattern abstraction.

**Configure Your Model:**

```csharp
using AppFactory.Framework.DataAccess.DynamoDB.Configuration;

public class UserModelConfig : IModelConfig<User>
{
    public void Configure(IModelConfigOptions<User> options)
    {
        options
            .PK(u => u.Id)
            .SK(u => u.Email)
            .PKPrefix("USER")
            .SKPrefix("EMAIL");
    }
}
```

**Use the Repository:**

```csharp
using AppFactory.Framework.DataAccess.DynamoDB.Repositories;

public class UserRepository : RepositoryBase<User>
{
    public UserRepository(
        IDynamoDBClientFactory dynamoDbFactory, 
        ILogger logger, 
        IModelConfig<User> modelConfig) 
        : base(dynamoDbFactory, logger, modelConfig)
    {
    }

    public async Task<User> GetByEmail(string email)
    {
        // Query using partition and sort key
        return await GetByPrimaryKey(
            _config.GetPrimaryKey(userId, email));
    }
}
```

### Azure Cosmos DB Data Access

AppFactory also supports Azure Cosmos DB with the same repository pattern for Azure-based serverless applications.

**Configure Your Model:**

```csharp
using AppFactory.Framework.DataAccess.CosmosDB.Configuration;

public class UserModelConfig : IModelConfig<User>
{
    public void Configure(IModelConfigOptions<User> options)
    {
        options
            .ContainerName("Users")
            .Id(u => u.Id)
            .PartitionKey(u => u.TenantId)
            .IdPrefix("USER")
            .PartitionKeyPrefix("TENANT");
    }
}
```

**Use the Repository:**

```csharp
using AppFactory.Framework.DataAccess.CosmosDB.Repositories;

public class UserRepository : RepositoryBase<User>
{
    public UserRepository(
        ICosmosDbClientFactory cosmosDbFactory, 
        ILogger logger, 
        IModelConfig<User> modelConfig) 
        : base(cosmosDbFactory, logger, modelConfig)
    {
    }

    public async Task<User> GetByEmail(string email)
    {
        var query = "SELECT * FROM c WHERE c.email = @email";
        var queryDef = CreateQuery(query)
            .WithParameter("@email", email);
        return await QuerySingle(queryDef);
    }
}
```

> **Note:** For detailed CosmosDB usage, see [AppFactory.Framework.DataAccess.CosmosDB README](src/AppFactory.Framework.DataAccess.CosmosDB/README.md)

### Lambda Message Handlers

Process SQS messages in Lambda functions.

```csharp
using AppFactory.Framework.Messaging.LambdaHandlers;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.Core;

public class OrderMessageProcessor : ILambdaMessageProcessor<OrderMessage>
{
    public async Task<bool> Process(OrderMessage message, CancellationToken cancellationToken)
    {
        // Process the order
        Console.WriteLine($"Processing order: {message.OrderId}");
        return true;
    }
}

public class ProcessOrderLambda : LambdaMessageHandlerBase<OrderMessage>
{
    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        await Handle(sqsEvent, context);
    }
}
```

## 🏗️ Serverless Architecture Benefits

With AppFactory, you can:

- **Focus on Business Logic** - No need to worry about server management, scaling, or infrastructure
- **Event-Driven by Default** - Built-in support for EventBridge and SQS makes microservices communication simple
- **Type-Safe Data Access** - Work with DynamoDB using strongly-typed models and LINQ-like queries
- **Simplified Lambda Development** - Base classes handle the boilerplate, you write the logic
- **Cost-Effective** - Pay only for what you use with serverless AWS services
- **Automatic Scaling** - Lambda and DynamoDB scale automatically with your load

## 📚 Complete Usage Example

Here's a complete example showing a CQRS flow for an order processing system:

```csharp
using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Application.Queries;
using AppFactory.Framework.Domain.Entities;
using AppFactory.Framework.Domain.ServiceResult;

// ============================================
// Domain Entities
// ============================================
public class Order : EntityWithTypedId<Guid>
{
    public string CustomerId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
}

public class OrderItem
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered
}

// ============================================
// Commands
// ============================================
public class PlaceOrderCommand : ICommand
{
    public string CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

public class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand>
{
    public async Task<CommandResult> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        // Validation
        if (string.IsNullOrEmpty(command.CustomerId))
        {
            return CommandResult.ErrorResult("CUSTOMER_REQUIRED", "Customer ID is required");
        }

        if (command.Items == null || !command.Items.Any())
        {
            return CommandResult.ErrorResult("EMPTY_ORDER", "Order must contain at least one item");
        }

        // Business logic
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = command.CustomerId,
            Items = command.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList(),
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };

        order.TotalAmount = order.Items.Sum(i => i.Price * i.Quantity);

        // Check business rules
        if (order.TotalAmount <= 0)
        {
            return CommandResult.ErrorResult(
                new Error("INVALID_AMOUNT", "Order total must be greater than zero"),
                FailureReason.DomainError
            );
        }

        // Save to database...
        // await _repository.AddAsync(order, cancellationToken);

        return CommandResult.Success(order.Id.ToString());
    }
}

// ============================================
// Queries
// ============================================
public class GetOrderByIdQuery : IQueryRequest
{
    public Guid OrderId { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
}

public class OrderItemDto
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        // Retrieve order from database...
        // var order = await _repository.GetByIdAsync(request.OrderId, cancellationToken);

        return new OrderDto
        {
            Id = request.OrderId,
            // Map properties...
        };
    }
}

// ============================================
// Usage in Application Layer
// ============================================
public class OrderService
{
    private readonly ICommandHandler<PlaceOrderCommand> _placeOrderHandler;
    private readonly IQueryHandler<GetOrderByIdQuery, OrderDto> _getOrderHandler;

    public OrderService(
        ICommandHandler<PlaceOrderCommand> placeOrderHandler,
        IQueryHandler<GetOrderByIdQuery, OrderDto> getOrderHandler)
    {
        _placeOrderHandler = placeOrderHandler;
        _getOrderHandler = getOrderHandler;
    }

    public async Task<string> PlaceOrderAsync(PlaceOrderCommand command)
    {
        var result = await _placeOrderHandler.Handle(command);

        if (result.IsFailure)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Message));
            throw new InvalidOperationException($"Failed to place order: {errors}");
        }

        return result.Id;
    }

    public async Task<OrderDto> GetOrderAsync(Guid orderId)
    {
        var query = new GetOrderByIdQuery { OrderId = orderId };
        return await _getOrderHandler.Handle(query);
    }
}
```

## 🏛️ Architecture Principles

AppFactory promotes clean architecture principles with a clear layered architecture:

### 📐 Clean Architecture Layers

AppFactory follows **Clean Architecture** principles with strict dependency rules ensuring maintainable, testable code:

```
┌─────────────────────────────────────────────────────────────┐
│                     Infrastructure Layer                     │
│  ┌────────────────────────────────────────────────────────┐  │
│  │           AppFactory.Framework.DataAccess.*            │  │
│  │     (DynamoDB, CosmosDB Repository Implementations)    │  │
│  └────────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────┐  │
│  │            AppFactory.Framework.Api                    │  │
│  │         (Lambda Handlers, API Gateway)                 │  │
│  └────────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────┐  │
│  │         AppFactory.Framework.EventBus.Aws              │  │
│  │       (EventBridge, SQS Implementations)               │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────────────┘
                       │ depends on ↓
┌──────────────────────┴──────────────────────────────────────┐
│                    Application Layer                         │
│  ┌────────────────────────────────────────────────────────┐  │
│  │         AppFactory.Framework.Application               │  │
│  │    (CQRS Handlers, Commands, Queries, Use Cases)       │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────────────┘
                       │ depends on ↓
┌──────────────────────┴──────────────────────────────────────┐
│                       Domain Layer                           │
│  ┌────────────────────────────────────────────────────────┐  │
│  │            AppFactory.Framework.Domain                 │  │
│  │    (Entities, Value Objects, Domain Services,          │  │
│  │     IRepository - following Dependency Inversion)      │  │
│  └────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

**Layer Responsibilities:**

#### 🏛️ Domain Layer (`AppFactory.Framework.Domain`)
- **Pure business logic** - No infrastructure dependencies
- **Entities & Value Objects** - Core domain models with behavior
- **Domain Services** - Business logic that doesn't belong to a single entity
- **Repository Interfaces** - Defines what persistence abstractions the domain needs (Dependency Inversion Principle)
- **Domain Events** - Represent state changes in the domain
- **Zero external dependencies** - Keeps business logic portable and testable

#### 🎯 Application Layer (`AppFactory.Framework.Application`)
- **Use Case Orchestration** - Coordinates domain objects to fulfill business use cases
- **CQRS Infrastructure** - Commands, Queries, Handlers, and Dispatcher
- **Application Services** - Application-specific business logic and workflows
- **DTOs & Mapping** - Data transfer objects for cross-boundary communication
- **Depends only on Domain** - One-way dependency ensures domain remains pure

#### 🔌 Infrastructure Layer (DataAccess, API, EventBus)
- **Repository Implementations** - Concrete implementations of `IRepository<T>` (DynamoDB, CosmosDB)
- **External Service Integrations** - AWS Lambda, EventBridge, SQS, database clients
- **Framework-Specific Code** - Dependency injection, configuration, logging
- **Depends on Application & Domain** - Implements interfaces defined by inner layers

**Dependency Rule:** Dependencies flow **inward only**. Infrastructure → Application → Domain. The domain layer has no knowledge of outer layers.

**Key Benefits:**
- ✅ **Testability** - Domain and Application layers are easily unit tested without infrastructure
- ✅ **Flexibility** - Swap implementations (DynamoDB ↔ CosmosDB) without changing business logic
- ✅ **Maintainability** - Clear separation of concerns makes changes predictable
- ✅ **Independent Deployment** - Infrastructure can evolve independently of business rules

---

### Separation of Concerns
- **Commands** modify state and return results
- **Queries** retrieve data without side effects
- Clear boundaries between read and write operations

### Single Responsibility
- Each handler has one clear purpose
- Commands and queries are focused and explicit

### Domain-Centric Design
- Business logic stays in the domain layer
- Entities encapsulate domain behavior
- Results communicate success or failure explicitly

### Explicit Intent
- Commands and queries express clear intentions
- Type-safe contracts between layers
- Compile-time verification of operations

### Testability
- Handlers can be unit tested in isolation
- Clear dependencies and contracts
- Easy to mock and stub

## 🔧 Best Practices

### Command Design
- Keep commands simple and focused on a single operation
- Include all necessary data in the command
- Use validation in the handler, not in the command itself
- Return meaningful error codes and messages

### Query Design
- Queries should never modify state
- Return DTOs, not domain entities
- Keep queries focused on specific use cases
- Use cancellation tokens for long-running queries

### Error Handling
- Use appropriate `FailureReason` values
- Provide clear, actionable error messages
- Include error codes for client-side handling
- Group related errors when validating multiple fields

### Entity Design
- Use typed IDs for type safety (e.g., `EntityWithTypedId<Guid>`)
- Keep entities focused on domain logic
- Use value objects for complex attributes
- Implement proper equality based on ID

### Serverless & AWS Best Practices
- **Keep Lambda functions focused** - One function per endpoint or event type
- **Use environment variables** - Configure queue URLs, table names, and EventBus names via environment variables
- **Optimize cold starts** - Keep dependencies minimal and use Lambda layers where appropriate
- **DynamoDB design** - Design partition keys carefully to avoid hot partitions
- **Event-driven architecture** - Use EventBridge for loosely coupled microservices communication
- **Async processing** - Use SQS for long-running or batch operations
- **Idempotency** - Ensure message and event handlers are idempotent
- **Observability** - Use structured logging for CloudWatch integration

## 🗂️ Project Structure

A typical serverless project using AppFactory might look like:

```
YourProject/
├── src/
│   ├── Domain/
│   │   ├── Commands/
│   │   │   ├── CreateUserCommand.cs
│   │   │   └── CreateUserCommandHandler.cs
│   │   ├── Queries/
│   │   │   ├── GetUserByIdQuery.cs
│   │   │   └── GetUserByIdQueryHandler.cs
│   │   ├── Entities/
│   │   │   └── User.cs
│   │   └── Events/
│   │       └── UserCreatedEvent.cs
│   ├── DataAccess/
│   │   ├── Repositories/
│   │   │   └── UserRepository.cs
│   │   └── Configuration/
│   │       └── UserModelConfig.cs
│   ├── Lambda.Api/
│   │   ├── CreateUserLambda.cs
│   │   ├── GetUserLambda.cs
│   │   └── serverless.yml
│   └── Lambda.Events/
│       ├── UserCreatedEventHandler.cs
│       └── ProcessOrderMessageHandler.cs
└── tests/
    └── UnitTests/
```

**AWS Resources:**
- Lambda functions for API endpoints (API Gateway → Lambda)
- Lambda functions for event processing (EventBridge → Lambda)
- Lambda functions for message processing (SQS → Lambda)
- DynamoDB tables for data persistence
- EventBridge for event-driven architecture
- SQS queues for async messaging

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

### How to Contribute

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Guidelines

- Follow existing code style and conventions
- Add unit tests for new features
- Update documentation as needed
- Keep pull requests focused on a single feature or fix

## 📄 License

Copyright © Sergey Kichuk. All rights reserved.

Licensed under the [MIT License](LICENSE).

## 👤 Author

**Sergey Kichuk**

- GitHub: [@exiton3](https://github.com/exiton3)

## 🔗 Links

- **GitHub Repository**: [https://github.com/exiton3/AppFactory](https://github.com/exiton3/AppFactory)
- **Issue Tracker**: [https://github.com/exiton3/AppFactory/issues](https://github.com/exiton3/AppFactory/issues)
- **NuGet Package**: [AppFactory.Framework.Domain](https://www.nuget.org/packages/AppFactory.Framework.Domain/)

## 📞 Support

If you encounter any issues or have questions:

- 🐛 [Report a bug](https://github.com/exiton3/AppFactory/issues/new?labels=bug)
- 💡 [Request a feature](https://github.com/exiton3/AppFactory/issues/new?labels=enhancement)
- 📖 Check existing [documentation](https://github.com/exiton3/AppFactory/wiki)
- ⭐ Star the project if you find it useful!

## 🙏 Acknowledgments

Built with ❤️ for the .NET community to simplify CQRS and DDD implementation.

---

**Happy Coding!** 🚀
