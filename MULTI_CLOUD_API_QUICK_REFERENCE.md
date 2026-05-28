# AppFactory Multi-Cloud API - Quick Reference

## 🚀 Installation

### AWS Lambda
```bash
dotnet add package AppFactory.Framework.Api.Aws
```

### Azure Functions
```bash
dotnet add package AppFactory.Framework.Api.Azure
```

### ASP.NET Core (Container Apps)
```bash
dotnet add package AppFactory.Framework.Api.AspNetCore
```

## 📝 Basic Usage

### 1. Define Your Models (Same for All Platforms)

```csharp
using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Api.Parsing.Configurations;

public class CreateUserCommand : ICommand
{
    [FromBody]
    public string Email { get; set; }
    
    [FromBody]
    public string Name { get; set; }
}

public class UserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}
```

### 2. Implement Processor (Same for All Platforms)

```csharp
using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Domain.ServiceResult;

public class CreateUserProcessor : IFunctionProcessor<CreateUserCommand, UserDto>
{
    private readonly ICommandHandler<CreateUserCommand> _handler;

    public CreateUserProcessor(ICommandHandler<CreateUserCommand> handler)
    {
        _handler = handler;
    }

    public async Task<Result<UserDto>> Process(
        CreateUserCommand request, 
        CancellationToken cancellationToken)
    {
        var result = await _handler.Handle(request, cancellationToken);
        
        if (result.IsFailure)
            return Result<UserDto>.Invalid(result.Errors);

        var dto = new UserDto 
        { 
            Id = result.Id, 
            Email = request.Email,
            Name = request.Name 
        };
        
        return Result<UserDto>.Ok(dto);
    }
}
```

### 3. Platform-Specific Entry Points

#### AWS Lambda

```csharp
using AppFactory.Framework.Api.Aws;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

public class CreateUserLambda : LambdaFunctionHandlerBase<CreateUserCommand, UserDto>
{
    protected override IStartup GetStartup() => new Startup();

    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request, 
        ILambdaContext context)
    {
        return await Handle(request, context);
    }
}
```

**serverless.yml:**
```yaml
service: user-service

provider:
  name: aws
  runtime: dotnet10
  region: us-east-1

functions:
  createUser:
    handler: UserService::CreateUserLambda::FunctionHandler
    events:
      - http:
          path: users
          method: post
```

#### Azure Functions

```csharp
using AppFactory.Framework.Api.Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

public class CreateUserFunction : AzureFunctionHandlerBase<CreateUserCommand, UserDto>
{
    protected override IStartup GetStartup() => new Startup();

    [Function("CreateUser")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users")] 
        HttpRequestData req,
        FunctionContext executionContext)
    {
        return await Handle(req, executionContext);
    }
}
```

**host.json:**
```json
{
  "version": "2.0",
  "extensionBundle": {
    "id": "Microsoft.Azure.Functions.ExtensionBundle",
    "version": "[4.*, 5.0.0)"
  }
}
```

#### ASP.NET Core (Container Apps)

```csharp
using AppFactory.Framework.Api.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add AppFactory services
builder.Services.AddAppFactoryApi(typeof(Program).Assembly);

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoints
app.MapCommand<CreateUserCommand, UserDto>("/api/users")
   .WithName("CreateUser")
   .WithOpenApi();

app.MapHealthChecks("/health");

app.Run();
```

**Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserService.dll"]
```

## 🔧 Dependency Injection Setup

### Startup.cs (Same for All Platforms)

```csharp
using AppFactory.Framework.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register CQRS
        services.AddCqrs(typeof(CreateUserCommandHandler).Assembly);
        
        // Register processors
        services.AddScoped<IFunctionProcessor<CreateUserCommand, UserDto>, CreateUserProcessor>();
        
        // Register repositories
        services.AddScoped<IRepository<User>, UserRepository>();
        
        // Register other services
        services.AddSingleton<IEmailService, EmailService>();
    }
}
```

## 🌐 Request/Response Examples

### Request Parsing

```csharp
// Path parameters
public class GetUserQuery
{
    [FromPath("userId")]
    public string UserId { get; set; }
}

// Query parameters
public class SearchUsersQuery
{
    [FromQuery("search")]
    public string SearchTerm { get; set; }
    
    [FromQuery("page")]
    public int Page { get; set; } = 1;
    
    [FromQuery("size")]
    public int Size { get; set; } = 10;
}

// Body parameters
public class UpdateUserCommand
{
    [FromPath("userId")]
    public string UserId { get; set; }
    
    [FromBody]
    public string Email { get; set; }
    
    [FromBody]
    public string Name { get; set; }
}
```

### Response Types

```csharp
// Success
return Result<UserDto>.Ok(userDto);

// Created with ID
return Result<UserDto>.Ok(userDto);

// Accepted (async processing)
return Result<UserDto>.Accepted(userDto);

// Validation error
return Result<UserDto>.Invalid(new List<Error>
{
    new Error("EMAIL_REQUIRED", "Email is required"),
    new Error("NAME_TOO_SHORT", "Name must be at least 2 characters")
});

// Not found
return Result<UserDto>.NotFound(new Error("USER_NOT_FOUND", "User not found"));

// Unauthorized
return Result<UserDto>.Unauthorized(new Error("UNAUTHORIZED", "Invalid token"));

// External system error
return Result<UserDto>.External(new Error("DATABASE_ERROR", "Database connection failed"));

// Unexpected error
return Result<UserDto>.Unexpected(new Error("UNKNOWN_ERROR", "An unexpected error occurred"));
```

## 📊 HTTP Status Code Mapping

| Result Type | HTTP Status | Use Case |
|-------------|-------------|----------|
| `Ok` | 200 OK | Successful operation |
| `Accepted` | 202 Accepted | Async processing started |
| `Invalid` | 400 Bad Request | Validation errors |
| `Unauthorized` | 401 Unauthorized | Authentication failed |
| `NotFound` | 404 Not Found | Resource doesn't exist |
| `External` | 503 Service Unavailable | External system failure |
| `Unexpected` | 500 Internal Server Error | Unhandled exception |

## 🚀 Deployment Commands

### AWS Lambda
```bash
# Using Serverless Framework
serverless deploy

# Using AWS SAM
sam build
sam deploy --guided

# Using AWS CLI
dotnet lambda deploy-function CreateUserFunction
```

### Azure Functions
```bash
# Create Function App
az functionapp create \
  --resource-group MyResourceGroup \
  --name MyFunctionApp \
  --storage-account mystorage \
  --functions-version 4 \
  --runtime dotnet-isolated

# Deploy
func azure functionapp publish MyFunctionApp
```

### Azure Container Apps
```bash
# Build and push to ACR
az acr build --registry myregistry --image myapi:latest .

# Deploy
az containerapp create \
  --name my-api \
  --resource-group MyResourceGroup \
  --image myregistry.azurecr.io/myapi:latest \
  --target-port 8080 \
  --ingress external
```

## 🧪 Testing

### Unit Test (Platform-Agnostic)

```csharp
public class CreateUserProcessorTests
{
    [Fact]
    public async Task Process_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var mockHandler = new Mock<ICommandHandler<CreateUserCommand>>();
        mockHandler
            .Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), default))
            .ReturnsAsync(CommandResult.Success("user-123"));
        
        var processor = new CreateUserProcessor(mockHandler.Object);
        var command = new CreateUserCommand 
        { 
            Email = "test@example.com", 
            Name = "Test User" 
        };
        
        // Act
        var result = await processor.Process(command, default);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("user-123", result.Data.Id);
        Assert.Equal("test@example.com", result.Data.Email);
    }
}
```

## 📚 Complete Example Projects

See the `/examples` folder for complete working examples:
- `examples/AWS.Lambda.UserService` - AWS Lambda example
- `examples/Azure.Functions.UserService` - Azure Functions example
- `examples/AspNetCore.UserService` - ASP.NET Core example

## 🔗 Resources

- [Migration Guide](MULTI_CLOUD_API_MIGRATION_GUIDE.md)
- [Implementation Summary](MULTI_CLOUD_API_IMPLEMENTATION_SUMMARY.md)
- [AWS Lambda README](src/AppFactory.Framework.Api.Aws/README.md)
- [Azure Functions README](src/AppFactory.Framework.Api.Azure/README.md)
- [ASP.NET Core README](src/AppFactory.Framework.Api.AspNetCore/README.md)
