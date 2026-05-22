# AppFactory.Framework.Application

CQRS Application Layer - Commands, Queries, and Handlers for Clean Architecture.

## Quick Reference

See the main [README](../../README.md) for complete documentation including:
- Commands and Command Handlers
- Queries and Query Handlers  
- Command Dispatcher
- Dependency Registration
- Best Practices
- Code Examples

## Installation

```bash
dotnet add package AppFactory.Framework.Application
```

## Quick Start

### 1. Define a Command

```csharp
using AppFactory.Framework.Application.Commands;

public class CreateUserCommand : ICommand
{
    public string Email { get; set; }
    public string Name { get; set; }
}
```

### 2. Implement Command Handler

```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    public async Task<CommandResult> Handle(CreateUserCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            return CommandResult.ErrorResult("INVALID_EMAIL", "Email is required");

        // Business logic...

        return CommandResult.Success(userId);
    }
}
```

### 3. Register Services

```csharp
services.AddCqrs(typeof(CreateUserCommandHandler).Assembly);
```

### 4. Use the Dispatcher

```csharp
public class UserService
{
    private readonly ICommandDispatcher _dispatcher;

    public async Task<string> CreateUser(string email, string name)
    {
        var command = new CreateUserCommand { Email = email, Name = name };
        var result = await _dispatcher.Dispatch(command);
        return result.Id;
    }
}
```

## Core Components

### Command Dispatcher
- **Purpose**: Dynamic command routing for scenarios where command types are determined at runtime
- **Use Cases**: SQS messages, EventBridge events, generic API endpoints
- **Registration**: Singleton lifetime

### Command Handlers
- **Interface**: `ICommandHandler<TCommand>`
- **Return Type**: `CommandResult` (uniform success/failure response)
- **Registration**: Scoped lifetime (per-request)
- **Best Practice**: One handler per command

### Query Handlers
- **Interface**: `IQueryHandler<TRequest, TResponse>`
- **Return Type**: Strongly-typed response (DTOs, not domain entities)
- **Registration**: Scoped lifetime (per-request)
- **Best Practice**: Direct injection (type-safe), no dispatcher needed

## Registration Options

```csharp
// Register all CQRS components
services.AddCqrs(typeof(MyHandler).Assembly);

// Register commands only
services.AddCommandHandlers(typeof(MyCommandHandler).Assembly);

// Register queries only
services.AddQueryHandlers(typeof(MyQueryHandler).Assembly);

// Register specific handlers
services.AddCommandHandler<CreateUserCommandHandler>();
services.AddQueryHandler<GetUserByIdQueryHandler>();

// Multiple assemblies
services.AddCqrs(assembly1, assembly2, assembly3);
```

## Architecture

```
???????????????????????????????????????????
?      Infrastructure Layer               ?
?  (API, DataAccess, EventBus)            ?
???????????????????????????????????????????
               ? depends on ?
???????????????????????????????????????????
?      Application Layer                  ?
?  (Commands, Queries, Handlers)          ?  ? YOU ARE HERE
???????????????????????????????????????????
               ? depends on ?
???????????????????????????????????????????
?      Domain Layer                       ?
?  (Entities, Value Objects, IRepository) ?
???????????????????????????????????????????
```

## Related Packages

- **AppFactory.Framework.Domain** - Core domain building blocks
- **AppFactory.Framework.DataAccess.DynamoDB** - DynamoDB repository implementation
- **AppFactory.Framework.DataAccess.CosmosDB** - Azure Cosmos DB repository implementation
- **AppFactory.Framework.Api** - Lambda function handlers
- **AppFactory.Framework.EventBus.Aws** - EventBridge integration

## License

MIT License - see [LICENSE](https://github.com/exiton3/AppFactory/blob/master/LICENSE)
