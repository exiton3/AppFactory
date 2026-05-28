# AppFactory.Framework.Api.Azure

[![NuGet](https://img.shields.io/nuget/v/AppFactory.Framework.Api.Azure.svg)](https://www.nuget.org/packages/AppFactory.Framework.Api.Azure/)

Azure Functions integration for the AppFactory framework.

## Overview

This package provides Azure Functions-specific implementations for building serverless APIs using the AppFactory CQRS pattern. It uses the **Azure Functions v4 isolated worker model** for maximum performance and flexibility.

## Features

- ✅ **Azure Functions v4 Isolated Worker** - Latest Azure Functions runtime
- ✅ **Automatic Request Parsing** - Path, query, and body parameters parsed automatically
- ✅ **Type-Safe Responses** - Strongly-typed response building with proper HTTP status codes
- ✅ **Error Handling** - Comprehensive error handling with problem details format
- ✅ **Performance Logging** - Built-in performance tracking
- ✅ **CORS Support** - Pre-configured CORS headers
- ✅ **Platform-Agnostic Core** - Share business logic across AWS, Azure, and ASP.NET Core

## Installation

```bash
dotnet add package AppFactory.Framework.Api.Azure
```

## Quick Start

### 1. Create an Azure Function

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using AppFactory.Framework.Api.Azure;

public class CreateUserFunction : AzureFunctionHandlerBase<CreateUserCommand, UserDto>
{
    protected override IStartup GetStartup()
    {
        return new Startup(); // Your DI configuration
    }

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

### 2. Implement a Processor

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
        
        var user = new UserDto { Id = result.Id, Email = request.Email };
        return Result<UserDto>.Ok(user);
    }
}
```

### 3. Configure Dependency Injection

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
    }
}
```

### 4. Configure host.json

```json
{
  "version": "2.0",
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "maxTelemetryItemsPerSecond": 20
      }
    }
  },
  "extensionBundle": {
    "id": "Microsoft.Azure.Functions.ExtensionBundle",
    "version": "[4.*, 5.0.0)"
  }
}
```

### 5. Configure local.settings.json

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

## Advanced Usage

### Multiple HTTP Methods

```csharp
public class UserFunction : AzureFunctionHandlerBase<UserCommand, UserDto>
{
    [Function("CreateUser")]
    public async Task<HttpResponseData> Create(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users")] 
        HttpRequestData req,
        FunctionContext context)
    {
        return await Handle(req, context);
    }

    [Function("GetUser")]
    public async Task<HttpResponseData> Get(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{userId}")] 
        HttpRequestData req,
        FunctionContext context)
    {
        return await Handle(req, context);
    }
}
```

### Route Parameters

Route parameters are automatically extracted and available in your request model:

```csharp
public class GetUserQuery
{
    [FromPath("userId")]
    public string UserId { get; set; }
}

[Function("GetUser")]
public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{userId}")] 
    HttpRequestData req,
    FunctionContext context)
{
    return await Handle(req, context);
}
```

### Query Parameters

```csharp
public class SearchUsersQuery
{
    [FromQuery("page")]
    public int Page { get; set; } = 1;
    
    [FromQuery("size")]
    public int Size { get; set; } = 10;
    
    [FromQuery("search")]
    public string SearchTerm { get; set; }
}
```

## Deployment

### Deploy to Azure

```bash
# Login to Azure
az login

# Create resource group
az group create --name MyResourceGroup --location eastus

# Create storage account
az storage account create --name mystorageaccount --resource-group MyResourceGroup

# Create Function App
az functionapp create \
  --resource-group MyResourceGroup \
  --consumption-plan-location eastus \
  --runtime dotnet-isolated \
  --functions-version 4 \
  --name MyFunctionApp \
  --storage-account mystorageaccount

# Deploy
func azure functionapp publish MyFunctionApp
```

## Best Practices

1. **Use Isolated Worker Model** - Better performance and dependency isolation
2. **Configure Application Insights** - Monitor performance and errors
3. **Use Managed Identity** - Secure access to Azure resources
4. **Set Appropriate Auth Levels** - Use `AuthorizationLevel.Function` for production
5. **Enable CORS** - Configure in Azure portal or host.json
6. **Use Durable Functions** - For long-running workflows (requires additional setup)

## Comparison with AWS Lambda

| Feature | AWS Lambda | Azure Functions |
|---------|-----------|-----------------|
| Base Class | `LambdaFunctionHandlerBase` | `AzureFunctionHandlerBase` |
| Request Type | `APIGatewayProxyRequest` | `HttpRequestData` |
| Response Type | `APIGatewayProxyResponse` | `HttpResponseData` |
| Context | `ILambdaContext` | `FunctionContext` |
| Processor Interface | `IFunctionProcessor<TRequest, TResponse>` | Same ✅ |
| Business Logic | Platform-agnostic ✅ | Platform-agnostic ✅ |

## See Also

- [AppFactory.Framework.Api.Aws](../AppFactory.Framework.Api.Aws/README.md) - AWS Lambda integration
- [AppFactory.Framework.Api.AspNetCore](../AppFactory.Framework.Api.AspNetCore/README.md) - ASP.NET Core integration
- [AppFactory Main Documentation](../../README.md)
- [Azure Functions Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/)
