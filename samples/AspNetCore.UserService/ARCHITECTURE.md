# AspNetCore.UserService - Architecture Guide

## Architecture Overview

This service follows **Vertical Slice Architecture** combined with **Domain-Driven Design (DDD)** and **Clean Architecture** principles.

## Project Structure

```
AspNetCore.UserService/
├── Features/                    # Vertical Slices - organized by feature/use case
│   └── Users/
│       ├── CreateUser/         # Everything needed for CreateUser feature
│       │   ├── CreateUserContracts.cs      # Request/Response DTOs
│       │   ├── CreateUserCommand.cs        # Command definition
│       │   ├── CreateUserCommandHandler.cs # Command handler (business logic)
│       │   ├── CreateUserProcessor.cs      # API processor
│       │   ├── CreateUserRequestMap.cs     # HTTP request mapping
│       │   └── CreateUserEndpoint.cs       # Endpoint configuration
│       └── GetUserById/        # Everything needed for GetUserById feature
│           ├── GetUserByIdQuery.cs
│           ├── GetUserByIdQueryHandler.cs
│           ├── GetUserByIdProcessor.cs
│           ├── GetUserByIdQueryMap.cs
│           └── GetUserByIdEndpoint.cs
│
├── Domain/                      # Domain Layer (core business logic)
│   └── Users/
│       ├── User.cs             # User aggregate root with business rules
│       └── IUserRepository.cs  # Repository interface (dependency inversion)
│
├── Infrastructure/              # Infrastructure Layer (implementation details)
│   └── Persistence/
│       └── UserRepository.cs   # Repository implementation
│
├── Contracts/                   # Shared contracts (DTOs for API)
│   └── Users/
│       └── UserDto.cs
│
└── Program.cs                   # Application entry point & composition root

```

## Architecture Principles

### 1. Vertical Slice Architecture

Each feature (use case) contains everything it needs:
- **Request/Response contracts** - What comes in/out of the API
- **Command/Query** - Business intent
- **Handler** - Business logic
- **Processor** - API orchestration
- **Request mapping** - HTTP → Domain
- **Endpoint** - HTTP configuration

**Benefits:**
- ✅ High cohesion - related code stays together
- ✅ Easy to find - everything for a feature is in one place
- ✅ Easy to change - changes are localized to a single slice
- ✅ Easy to delete - remove a feature by deleting its folder
- ✅ Team scalability - teams can own specific features

### 2. Domain-Driven Design (DDD)

**Domain Layer** (`Domain/Users/`):
- Contains business entities with encapsulated business rules
- Rich domain model with behavior (not anemic)
- Repository interfaces (dependency inversion)
- Example: `User.Create()` enforces email/name validation

**Ubiquitous Language:**
- Commands: `CreateUserCommand`, `UpdateUserCommand`
- Queries: `GetUserByIdQuery`
- Domain Events: `UserCreatedEvent` (future)

### 3. Clean Architecture

**Dependency Flow:**
```
Features → Domain ← Infrastructure
    ↓
Contracts
```

- **Domain** has no dependencies (pure business logic)
- **Infrastructure** depends on Domain (implements interfaces)
- **Features** depend on Domain & Contracts
- **Dependency Inversion:** Domain defines `IUserRepository`, Infrastructure implements it

### 4. CQRS (Command Query Responsibility Segregation)

**Commands** (write operations):
- `CreateUserCommand` → `CreateUserCommandHandler`
- Modify state, return success/failure
- Can have side effects

**Queries** (read operations):
- `GetUserByIdQuery` → `GetUserByIdQueryHandler`
- Read-only, return data
- No side effects

## Adding a New Feature

### Example: Add "UpdateUser" feature

1. **Create feature folder:**
   ```
   Features/Users/UpdateUser/
   ```

2. **Create contracts:**
   ```csharp
   // UpdateUserContracts.cs
   public sealed class UpdateUserRequest
   {
       public string UserId { get; set; }
       public string Name { get; set; }
   }
   ```

3. **Create command:**
   ```csharp
   // UpdateUserCommand.cs
   public sealed class UpdateUserCommand : ICommand
   {
       public string UserId { get; set; }
       public string Name { get; set; }
   }
   ```

4. **Create handler:**
   ```csharp
   // UpdateUserCommandHandler.cs
   public sealed class UpdateUserCommandHandler : CommandHandler<UpdateUserCommand>
   {
       private readonly IUserRepository _repository;
       
       protected override async Task<CommandResult> HandleCommand(
           UpdateUserCommand command, 
           CancellationToken ct)
       {
           var user = await _repository.GetByIdAsync(command.UserId, ct);
           if (user == null)
               return Failure("NOT_FOUND", "User not found");
           
           user.Update(command.Name); // Domain method
           await _repository.UpdateAsync(user, ct);
           
           return Success(user.Id);
       }
   }
   ```

5. **Create processor, mapping, and endpoint** (similar to CreateUser)

6. **Register in Program.cs:**
   ```csharp
   app.MapUpdateUserEndpoint();
   ```

## Testing Strategy

### Unit Tests (by layer):
```
Tests/
├── Domain.Tests/
│   └── Users/
│       └── UserTests.cs              # Test business rules
├── Features.Tests/
│   └── Users/
│       ├── CreateUser/
│       │   └── CreateUserHandlerTests.cs
│       └── GetUserById/
│           └── GetUserByIdHandlerTests.cs
└── Infrastructure.Tests/
    └── Persistence/
        └── UserRepositoryTests.cs
```

### Integration Tests:
```
IntegrationTests/
└── Features/
    └── Users/
        ├── CreateUserEndpointTests.cs
        └── GetUserByIdEndpointTests.cs
```

## Benefits of This Architecture

1. **Maintainability**
   - Changes are localized to single feature slices
   - Clear separation of concerns
   - Easy to understand and navigate

2. **Testability**
   - Domain logic is pure and easy to test
   - Handlers are isolated and testable
   - Infrastructure is swappable (in-memory → real DB)

3. **Scalability**
   - Features can be developed independently
   - Teams can own specific feature areas
   - Easy to extract features to microservices

4. **Flexibility**
   - Different features can use different patterns
   - Easy to evolve architecture per feature
   - Technology choices can vary by slice

## Key Patterns Used

- **Vertical Slice Architecture** - Feature-based organization
- **CQRS** - Command/Query separation
- **Repository Pattern** - Data access abstraction
- **Dependency Inversion** - Domain defines interfaces
- **Rich Domain Model** - Business logic in entities
- **API Processor Pattern** - HTTP orchestration layer

## Migration from Layered Architecture

**Before (Layered):**
```
Application/
  Commands/
  Queries/
  DTOs/
Domain/
Infrastructure/
```

**After (Vertical Slices):**
```
Features/
  Users/
    CreateUser/    # All layers for this feature
    GetUserById/   # All layers for this feature
```

**Migration steps:**
1. ✅ Created `Features/` folder structure
2. ✅ Moved CreateUser to vertical slice
3. ✅ Moved GetUserById to vertical slice
4. ✅ Organized Domain by aggregate
5. ✅ Moved Infrastructure to Persistence
6. ✅ Created Contracts for DTOs
7. ✅ Updated Program.cs with feature endpoints

## References

- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [CQRS](https://martinfowler.com/bliki/CQRS.html)
