# AppFactory.Framework.Api.AspNetCore

[![NuGet](https://img.shields.io/nuget/v/AppFactory.Framework.Api.AspNetCore.svg)](https://www.nuget.org/packages/AppFactory.Framework.Api.AspNetCore/)

ASP.NET Core Minimal API integration for the AppFactory framework. Perfect for Azure Container Apps, Kubernetes, and traditional web hosting.

## Overview

This package provides ASP.NET Core-specific implementations for building APIs using the AppFactory CQRS pattern. It leverages **ASP.NET Core Minimal APIs** for lightweight, high-performance endpoints.

## Features

- ✅ **Minimal API Support** - Clean, concise endpoint definitions
- ✅ **CQRS Integration** - Map commands and queries to HTTP endpoints
- ✅ **Automatic Request Parsing** - Path, query, and body parameters parsed automatically
- ✅ **Type-Safe Responses** - Strongly-typed response building with proper HTTP status codes
- ✅ **Error Handling Middleware** - Global exception handling with problem details
- ✅ **Request Logging Middleware** - Performance tracking and request logging
- ✅ **OpenAPI/Swagger Support** - Auto-generate API documentation
- ✅ **Health Checks** - Built-in health check support
- ✅ **Platform-Agnostic Core** - Share business logic across AWS, Azure, and ASP.NET Core

## Installation

```bash
dotnet add package AppFactory.Framework.Api.AspNetCore
```

## Quick Start

### 1. Create Program.cs

```csharp
using AppFactory.Framework.Api.AspNetCore.Extensions;
using AppFactory.Framework.Api.AspNetCore.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add AppFactory services
builder.Services.AddAppFactoryApi(typeof(Program).Assembly);

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Map CQRS endpoints
app.MapCommand<CreateUserCommand, UserDto>("/api/users")
   .WithName("CreateUser")
   .WithOpenApi();

app.MapQuery<GetUserByIdQuery, UserDto>("/api/users/{userId}")
   .WithName("GetUser")
   .WithOpenApi();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .WithName("HealthCheck");

app.Run();
```

### 2. Define Your Models

```csharp
using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Application.Queries;
using AppFactory.Framework.Api.Parsing.Configurations;

public class CreateUserCommand : ICommand
{
    [FromBody]
    public string Email { get; set; }
    
    [FromBody]
    public string Name { get; set; }
}

public class GetUserByIdQuery : IQueryRequest
{
    [FromPath("userId")]
    public string UserId { get; set; }
}

public class UserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}
```

### 3. Implement Processors

```csharp
using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Domain.ServiceResult;

public class CreateUserProcessor : IFunctionProcessor<CreateUserCommand, UserDto>
{
    private readonly ICommandHandler<CreateUserCommand> _commandHandler;
    
    public CreateUserProcessor(ICommandHandler<CreateUserCommand> commandHandler)
    {
        _commandHandler = commandHandler;
    }
    
    public async Task<Result<UserDto>> Process(
        CreateUserCommand request, 
        CancellationToken cancellationToken)
    {
        var result = await _commandHandler.Handle(request, cancellationToken);
        
        if (result.IsFailure)
        {
            return Result<UserDto>.Invalid(result.Errors);
        }
        
        var user = new UserDto 
        { 
            Id = result.Id, 
            Email = request.Email,
            Name = request.Name 
        };
        
        return Result<UserDto>.Ok(user);
    }
}

// Register in Program.cs or Startup
builder.Services.AddScoped<IFunctionProcessor<CreateUserCommand, UserDto>, CreateUserProcessor>();
```

## Advanced Usage

### Multiple Endpoints with Different HTTP Methods

```csharp
// Create user (POST)
app.MapCommand<CreateUserCommand, UserDto>("/api/users")
   .WithName("CreateUser");

// Get user by ID (GET)
app.MapQuery<GetUserByIdQuery, UserDto>("/api/users/{userId}")
   .WithName("GetUser");

// Update user (PUT)
app.MapCqrsEndpoint<UpdateUserCommand, UserDto>("/api/users/{userId}", "PUT")
   .WithName("UpdateUser");

// Delete user (DELETE)
app.MapCqrsEndpoint<DeleteUserCommand, UserDto>("/api/users/{userId}", "DELETE")
   .WithName("DeleteUser");

// Search users (GET with query params)
app.MapQuery<SearchUsersQuery, PagedResult<UserDto>>("/api/users")
   .WithName("SearchUsers");
```

### Custom Middleware

```csharp
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(authHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
            return;
        }

        await _next(context);
    }
}

// Use in app
app.UseMiddleware<AuthenticationMiddleware>();
```

### CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors();
```

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("database", () => HealthCheckResult.Healthy())
    .AddCheck("external-api", () => HealthCheckResult.Healthy());

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");
```

### Application Insights

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

## Deployment to Azure Container Apps

### 1. Create Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["YourApi/YourApi.csproj", "YourApi/"]
RUN dotnet restore "YourApi/YourApi.csproj"
COPY . .
WORKDIR "/src/YourApi"
RUN dotnet build "YourApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YourApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YourApi.dll"]
```

### 2. Deploy to Azure Container Apps

```bash
# Login to Azure
az login

# Create resource group
az group create --name MyResourceGroup --location eastus

# Create container registry
az acr create --resource-group MyResourceGroup --name myregistry --sku Basic

# Build and push image
az acr build --registry myregistry --image myapi:latest .

# Create Container App environment
az containerapp env create \
  --name my-environment \
  --resource-group MyResourceGroup \
  --location eastus

# Deploy Container App
az containerapp create \
  --name my-api \
  --resource-group MyResourceGroup \
  --environment my-environment \
  --image myregistry.azurecr.io/myapi:latest \
  --target-port 8080 \
  --ingress external \
  --registry-server myregistry.azurecr.io \
  --query properties.configuration.ingress.fqdn
```

### 3. Configure appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:8080"
      }
    }
  }
}
```

## Performance Optimization

### 1. Response Caching

```csharp
builder.Services.AddResponseCaching();
app.UseResponseCaching();

app.MapQuery<GetUserByIdQuery, UserDto>("/api/users/{userId}")
   .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(5)));
```

### 2. Output Caching (.NET 10)

```csharp
builder.Services.AddOutputCache();
app.UseOutputCache();

app.MapQuery<GetUserByIdQuery, UserDto>("/api/users/{userId}")
   .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(5)));
```

### 3. Compression

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

app.UseResponseCompression();
```

## Best Practices

1. **Use Minimal APIs** - Faster startup and better performance than controllers
2. **Enable Health Checks** - Monitor application health
3. **Configure Logging** - Use structured logging for better observability
4. **Use HTTPS** - Always encrypt traffic in production
5. **Implement Rate Limiting** - Protect against abuse
6. **Enable CORS Carefully** - Only allow trusted origins
7. **Use Application Insights** - Monitor performance and errors
8. **Containerize** - Deploy to Azure Container Apps or Kubernetes

## Comparison with Serverless

| Feature | AWS Lambda | Azure Functions | ASP.NET Core |
|---------|-----------|-----------------|---------------|
| **Hosting** | Serverless | Serverless | Container/VM |
| **Cold Start** | Yes | Yes | No |
| **Always On** | No | Optional | Yes |
| **Custom Runtime** | Limited | Limited | Full Control ✅ |
| **WebSocket Support** | Limited | Limited | Full Support ✅ |
| **Cost Model** | Pay-per-request | Pay-per-request | Pay-for-runtime |
| **Ideal For** | Event-driven | Event-driven | High-traffic APIs ✅ |

## See Also

- [AppFactory.Framework.Api.Aws](../AppFactory.Framework.Api.Aws/README.md) - AWS Lambda integration
- [AppFactory.Framework.Api.Azure](../AppFactory.Framework.Api.Azure/README.md) - Azure Functions integration
- [AppFactory Main Documentation](../../README.md)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Azure Container Apps Documentation](https://learn.microsoft.com/en-us/azure/container-apps/)
