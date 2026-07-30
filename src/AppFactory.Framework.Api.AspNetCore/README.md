# AppFactory.Framework.Api.AspNetCore

[![NuGet](https://img.shields.io/nuget/v/AppFactory.Framework.Api.AspNetCore.svg)](https://www.nuget.org/packages/AppFactory.Framework.Api.AspNetCore/)

ASP.NET Core Minimal API integration for the AppFactory framework.

## Overview

This package provides ASP.NET Core-specific implementations for building APIs using AppFactory request parsing and processor-based execution.

## Features

- Minimal API support with clean endpoint declarations
- Endpoint config classes (REPR style) via `EndpointConfig<TRequest, TResponse>`
- Staged fluent endpoint configuration
- Automatic request parsing (path/query/body)
- Generic request execution pipeline via endpoint request handlers
- Dedicated response mapping layer
- Error handling and request logging middleware support
- OpenAPI/Swagger support
- Health check support
- Shared core behavior with AWS and Azure integrations

## Installation

```bash
dotnet add package AppFactory.Framework.Api.AspNetCore
```

## Quick Start

### 1. Configure Program.cs

```csharp
using AppFactory.Framework.Api.AspNetCore.Extensions;
using AppFactory.Framework.Api.AspNetCore.Middleware;
using AppFactory.Framework.Api.Parsing.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppFactoryApi(typeof(Program).Assembly);

// Register parse maps
builder.Services.AddSingleton<IParseModelMap, CreateUserRequestMap>();

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Discover endpoint config classes from assembly
app.MapEndpointConfigs(typeof(Program).Assembly);

app.MapHealthChecks("/health");

app.Run();
```

### 2. Define Request/Response Contracts

```csharp
public sealed class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class CreateUserResponse
{
    public UserDto User { get; set; } = null!;
}

public sealed class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
```

### 3. Define Parse Map

```csharp
using AppFactory.Framework.Api.Parsing.Configurations;

public sealed class CreateUserRequestMap : ParseModelMap<CreateUserRequest>
{
    public CreateUserRequestMap()
    {
        Map(x => x.Email, "email").FromBody();
        Map(x => x.Name, "name").FromBody();
    }
}
```

### 4. Define Endpoint Config Class

```csharp
using AppFactory.Framework.Api.AspNetCore.Endpoints;
using Microsoft.AspNetCore.Http;

public sealed class CreateUserEndpoint : EndpointConfig<CreateUserRequest, CreateUserResponse>
{
    protected override void Configure()
    {
        Post("/api/users")
            .Name("CreateUser")
            .Summary("Create a new user")
            .Description("Creates a new user with the specified email and name")
            .Tags("Users")
            .Security()
            .RequireAuthorization()
            .Produces<CreateUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .ConfigureRoute(route => route.WithOpenApi());
    }
}
```

### 5. Implement Processor

```csharp
using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Domain.ServiceResult;

public sealed class CreateUserProcessor : IFunctionProcessor<CreateUserRequest, CreateUserResponse>
{
    public async Task<Result<CreateUserResponse>> Process(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        // Your business logic here
        var dto = new UserDto
        {
            Id = Guid.NewGuid().ToString("N"),
            Email = request.Email,
            Name = request.Name
        };

        return Result<CreateUserResponse>.Ok(new CreateUserResponse { User = dto });
    }
}
```

## Staged Fluent Configuration

`EndpointConfig<TRequest, TResponse>` uses staged fluent interfaces:

1. Route step: `Get/Post/Put/Patch/Delete`
2. Metadata step: `Name`, `Summary`, `Description`, `Tags`
3. Security step: `Security().AllowAnonymous()` or `Security().RequireAuthorization(...)`
4. Final step: `Produces(...)`, `ConfigureRoute(...)`

### Security Methods

- `RequireAuthorization()`
- `RequireAuthorization(string policy)`
- `RequireAuthorization(Action<AuthorizationPolicyBuilder> policyBuilder)`
- `AllowAnonymous()`

If both allow-anonymous and require-authorization are configured for the same endpoint, the framework throws `InvalidOperationException` to prevent silent misconfiguration.

## Runtime Pipeline (Separated Concerns)

The ASP.NET Core implementation is split into focused components:

1. Route mapping: `MapEndpointRoute<TRequest, TResponse>()`
2. Request execution: `IEndpointRequestHandler<TRequest, TResponse>`
3. Response mapping: `IEndpointResponseMapper<TResponse>`

This keeps endpoint declaration separate from execution behavior and response translation.

## Direct Mapping APIs

When you do not need endpoint config discovery, you can map directly:

```csharp
app.MapEndpointRoute<CreateUserRequest, CreateUserResponse>("/api/users", "POST");
app.MapCommand<CreateUserRequest, CreateUserResponse>("/api/users");
app.MapQuery<GetUserByIdQuery, UserDto>("/api/users/{userId}");
```

`MapEndpoint<TRequest, TResponse>()` is kept as a backward-compatible alias for `MapEndpointRoute<TRequest, TResponse>()`.

## Comparison with Serverless

| Feature | AWS Lambda | Azure Functions | ASP.NET Core |
|---------|-----------|-----------------|---------------|
| Hosting | Serverless | Serverless | Container/VM |
| Cold Start | Yes | Yes | No |
| Always On | No | Optional | Yes |
| Runtime Control | Limited | Limited | Full |
| Ideal For | Event-driven | Event-driven | High-traffic APIs |

## See Also

- [AppFactory.Framework.Api.Aws](../AppFactory.Framework.Api.Aws/README.md)
- [AppFactory.Framework.Api.Azure](../AppFactory.Framework.Api.Azure/README.md)
- [AppFactory Main Documentation](../../README.md)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
