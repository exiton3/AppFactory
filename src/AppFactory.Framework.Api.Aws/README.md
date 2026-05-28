# AppFactory.Framework.Api.Aws

[![NuGet](https://img.shields.io/nuget/v/AppFactory.Framework.Api.Aws.svg)](https://www.nuget.org/packages/AppFactory.Framework.Api.Aws/)

AWS Lambda and API Gateway integration for the AppFactory framework.

## Overview

This package provides AWS Lambda-specific implementations for building serverless APIs using the AppFactory CQRS pattern. It includes:

- **LambdaFunctionHandlerBase** - Base class for Lambda functions with API Gateway integration
- **ApiGatewayRequestContext** - Request adapter for API Gateway proxy requests
- **ApiGatewayResponseBuilder** - Response builder for API Gateway proxy responses

## Installation

```bash
dotnet add package AppFactory.Framework.Api.Aws
```

## Usage

### Create a Lambda Function Handler

```csharp
using AppFactory.Framework.Api.Aws;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

public class CreateUserLambda : LambdaFunctionHandlerBase<CreateUserCommand, UserDto>
{
    protected override IStartup GetStartup()
    {
        return new Startup(); // Your DI configuration
    }

    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request, 
        ILambdaContext context)
    {
        return await Handle(request, context);
    }
}
```

### Implement a Processor

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
        
        // Map to DTO
        var user = new UserDto { Id = result.Id, Email = request.Email };
        return Result<UserDto>.Ok(user);
    }
}
```

### Configure Dependency Injection

```csharp
using AppFactory.Framework.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register CQRS handlers
        services.AddCqrs(typeof(CreateUserCommandHandler).Assembly);
        
        // Register processor
        services.AddScoped<IFunctionProcessor<CreateUserCommand, UserDto>, CreateUserProcessor>();
        
        // Register repositories, services, etc.
    }
}
```

## Features

- ✅ **Automatic Request Parsing** - Path, query, and body parameters parsed automatically
- ✅ **Type-Safe Responses** - Strongly-typed response building with proper HTTP status codes
- ✅ **Error Handling** - Comprehensive error handling with problem details format
- ✅ **Performance Logging** - Built-in performance tracking
- ✅ **CORS Support** - Pre-configured CORS headers
- ✅ **Platform-Agnostic Core** - Share business logic across AWS, Azure, and ASP.NET Core

## Migration from AppFactory.Framework.Api

If you're using the legacy `AppFactory.Framework.Api` package:

1. Install `AppFactory.Framework.Api.Aws`
2. Change namespace from `AppFactory.Framework.Api.LambdaFunctionHandlers` to `AppFactory.Framework.Api.Aws`
3. Update `ILambdaProcessor<TRequest, TResponse>` to `IFunctionProcessor<TRequest, TResponse>`
4. Everything else remains the same!

## See Also

- [AppFactory.Framework.Api.Azure](../AppFactory.Framework.Api.Azure/README.md) - Azure Functions integration
- [AppFactory.Framework.Api.AspNetCore](../AppFactory.Framework.Api.AspNetCore/README.md) - ASP.NET Core integration
- [AppFactory Main Documentation](../../README.md)
