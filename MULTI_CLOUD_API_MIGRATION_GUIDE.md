# AppFactory API Layer - Multi-Cloud Migration Guide

## Overview

AppFactory v10.2.0 introduces **multi-cloud API support**, allowing you to build serverless and containerized APIs across AWS Lambda, Azure Functions, and ASP.NET Core with minimal code changes.

## What's New?

### ✨ Platform-Agnostic Core
- **IHttpRequestContext** - Unified request abstraction
- **IHttpResponseBuilder** - Unified response builder
- **IFunctionProcessor<TRequest, TResponse>** - Platform-agnostic processor interface

### 📦 New Packages

| Package | Purpose | Deployment Target |
|---------|---------|------------------|
| `AppFactory.Framework.Api.Aws` | AWS Lambda integration | AWS Lambda + API Gateway |
| `AppFactory.Framework.Api.Azure` | Azure Functions integration | Azure Functions (v4 isolated) |
| `AppFactory.Framework.Api.AspNetCore` | ASP.NET Core integration | Azure Container Apps, Kubernetes, VMs |

## Migration Path

### From `AppFactory.Framework.Api` (Legacy)

#### Before (AWS Lambda Only)
```csharp
using AppFactory.Framework.Api.LambdaFunctionHandlers;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

public class CreateUserLambda : LambdaFunctionHandlerBase<CreateUserCommand, UserDto>
{
    protected override IStartup GetStartup() => new Startup();

    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request, 
        ILambdaContext context)
    {
        return await Handle(request, context);
    }
}

public class CreateUserProcessor : ILambdaProcessor<CreateUserCommand, UserDto>
{
    // Implementation
}
```

#### After (Multi-Cloud)
```csharp
// AWS Lambda
using AppFactory.Framework.Api.Aws;

public class CreateUserLambda : LambdaFunctionHandlerBase<CreateUserCommand, UserDto>
{
    // Same as before! No changes needed
}

// Azure Functions
using AppFactory.Framework.Api.Azure;

public class CreateUserFunction : AzureFunctionHandlerBase<CreateUserCommand, UserDto>
{
    protected override IStartup GetStartup() => new Startup();

    [Function("CreateUser")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        FunctionContext context)
    {
        return await Handle(req, context);
    }
}

// ASP.NET Core
using AppFactory.Framework.Api.AspNetCore.Extensions;

var app = WebApplication.CreateBuilder(args).Build();
app.MapCommand<CreateUserCommand, UserDto>("/api/users");
app.Run();

// Processor (works everywhere!)
using AppFactory.Framework.Api.Abstractions;

public class CreateUserProcessor : IFunctionProcessor<CreateUserCommand, UserDto>
{
    // Same implementation works across AWS, Azure, and ASP.NET Core!
}
```

## Step-by-Step Migration

### Step 1: Update Package References

```xml
<!-- Old -->
<PackageReference Include="AppFactory.Framework.Api" Version="10.1.0" />

<!-- New: Choose your platform(s) -->
<PackageReference Include="AppFactory.Framework.Api.Aws" Version="10.2.0" />
<!-- AND/OR -->
<PackageReference Include="AppFactory.Framework.Api.Azure" Version="10.2.0" />
<!-- AND/OR -->
<PackageReference Include="AppFactory.Framework.Api.AspNetCore" Version="10.2.0" />
```

### Step 2: Update Namespaces

```csharp
// Old
using AppFactory.Framework.Api.LambdaFunctionHandlers;

// New (AWS)
using AppFactory.Framework.Api.Aws;

// New (Azure)
using AppFactory.Framework.Api.Azure;

// New (ASP.NET Core)
using AppFactory.Framework.Api.AspNetCore.Extensions;
```

### Step 3: Update Processor Interface

```csharp
// Old
using AppFactory.Framework.Api.LambdaFunctionHandlers;
public class MyProcessor : ILambdaProcessor<TRequest, TResponse>
{
    // Implementation
}

// New (platform-agnostic)
using AppFactory.Framework.Api.Abstractions;
public class MyProcessor : IFunctionProcessor<TRequest, TResponse>
{
    // Same implementation!
}
```

### Step 4: No Changes to Business Logic!

Your command handlers, query handlers, repositories, and domain models remain **100% unchanged**. This is the power of clean architecture!

## Multi-Cloud Deployment Scenarios

### Scenario 1: Gradual Migration (AWS → Azure)

**Phase 1: Run on AWS Lambda**
```csharp
// Use AppFactory.Framework.Api.Aws
public class MyLambda : LambdaFunctionHandlerBase<TRequest, TResponse>
```

**Phase 2: Deploy to Both AWS and Azure**
```csharp
// AWS Lambda (existing)
public class MyLambda : LambdaFunctionHandlerBase<TRequest, TResponse>

// Azure Functions (new)
public class MyFunction : AzureFunctionHandlerBase<TRequest, TResponse>

// Share processor implementation!
public class MyProcessor : IFunctionProcessor<TRequest, TResponse>
```

**Phase 3: Migrate to Azure Container Apps**
```csharp
// Switch to ASP.NET Core for long-running services
var app = WebApplication.CreateBuilder(args).Build();
app.MapCommand<CreateCommand, Response>("/api/create");
app.Run();
```

### Scenario 2: Hybrid Cloud Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     API Gateway                          │
│                  (Route by Region)                       │
└────────────┬────────────────────────────┬───────────────┘
             │                            │
             ▼                            ▼
    ┌────────────────┐          ┌────────────────┐
    │   AWS Lambda   │          │ Azure Functions│
    │   (US Region)  │          │   (EU Region)  │
    └────────────────┘          └────────────────┘
             │                            │
             └────────────┬───────────────┘
                          ▼
                ┌──────────────────┐
                │  Shared Business │
                │      Logic       │
                │  (IFunctionProc) │
                └──────────────────┘
```

### Scenario 3: Microservices with Mixed Hosting

```
Service A (Event-Driven) → AWS Lambda
Service B (Event-Driven) → Azure Functions
Service C (High-Traffic API) → ASP.NET Core (Container Apps)
Service D (WebSocket) → ASP.NET Core (AKS)
```

All services share the **same CQRS infrastructure** and **domain logic**!

## Backward Compatibility

The legacy `AppFactory.Framework.Api` package is **still supported** but marked as deprecated:

```csharp
[Obsolete("Use AppFactory.Framework.Api.Aws for AWS Lambda. " +
          "This package will be removed in v11.0.0")]
```

### Breaking Changes: None! 🎉

The old API continues to work. You can migrate at your own pace.

## Performance Considerations

### Cold Start Times

| Platform | Cold Start | Warm Start | Notes |
|----------|-----------|------------|-------|
| AWS Lambda | ~200-500ms | ~10-50ms | Use provisioned concurrency |
| Azure Functions | ~300-600ms | ~10-50ms | Use premium plan for always-warm |
| ASP.NET Core | 0ms (always warm) | ~5-20ms | Best for high-traffic |

### When to Use Each Platform

#### Use **AWS Lambda** When:
- ✅ Already on AWS infrastructure
- ✅ Event-driven architecture (S3, DynamoDB streams, etc.)
- ✅ Sporadic traffic patterns
- ✅ Cost optimization is critical

#### Use **Azure Functions** When:
- ✅ Already on Azure infrastructure
- ✅ Event-driven architecture (Event Grid, Service Bus, etc.)
- ✅ Integration with Azure services (Cosmos DB, Storage, etc.)
- ✅ Need Durable Functions for workflows

#### Use **ASP.NET Core** When:
- ✅ High-traffic, low-latency requirements
- ✅ WebSocket or SignalR support needed
- ✅ Long-running connections
- ✅ Full control over runtime and dependencies
- ✅ Predictable pricing model

## Example: Same Code, Three Platforms

### Shared Processor (Works Everywhere)
```csharp
using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Domain.ServiceResult;

public class CreateOrderProcessor : IFunctionProcessor<CreateOrderCommand, OrderDto>
{
    private readonly ICommandHandler<CreateOrderCommand> _handler;

    public CreateOrderProcessor(ICommandHandler<CreateOrderCommand> handler)
    {
        _handler = handler;
    }

    public async Task<Result<OrderDto>> Process(
        CreateOrderCommand request, 
        CancellationToken cancellationToken)
    {
        var result = await _handler.Handle(request, cancellationToken);
        
        if (result.IsFailure)
            return Result<OrderDto>.Invalid(result.Errors);

        return Result<OrderDto>.Ok(new OrderDto { Id = result.Id });
    }
}
```

### AWS Lambda Deployment
```csharp
using AppFactory.Framework.Api.Aws;

public class CreateOrderLambda : LambdaFunctionHandlerBase<CreateOrderCommand, OrderDto>
{
    protected override IStartup GetStartup() => new Startup();

    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request, 
        ILambdaContext context) => await Handle(request, context);
}
```

### Azure Functions Deployment
```csharp
using AppFactory.Framework.Api.Azure;

public class CreateOrderFunction : AzureFunctionHandlerBase<CreateOrderCommand, OrderDto>
{
    protected override IStartup GetStartup() => new Startup();

    [Function("CreateOrder")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        FunctionContext context) => await Handle(req, context);
}
```

### ASP.NET Core Deployment
```csharp
using AppFactory.Framework.Api.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAppFactoryApi(typeof(Program).Assembly);

var app = builder.Build();
app.MapCommand<CreateOrderCommand, OrderDto>("/api/orders");
app.Run();
```

**Result:** Same business logic, three different deployment models! 🚀

## Testing Strategy

```csharp
// Unit test your processor (platform-agnostic)
public class CreateOrderProcessorTests
{
    [Fact]
    public async Task Process_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var handler = new Mock<ICommandHandler<CreateOrderCommand>>();
        handler.Setup(x => x.Handle(It.IsAny<CreateOrderCommand>(), default))
               .ReturnsAsync(CommandResult.Success("order-123"));

        var processor = new CreateOrderProcessor(handler.Object);

        // Act
        var result = await processor.Process(new CreateOrderCommand(), default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("order-123", result.Data.Id);
    }
}
```

No need to test AWS/Azure/ASP.NET Core-specific code - the framework handles that!

## Migration Checklist

- [ ] Update package references to `AppFactory.Framework.Api.Aws/Azure/AspNetCore`
- [ ] Change `ILambdaProcessor` to `IFunctionProcessor`
- [ ] Update namespaces from `AppFactory.Framework.Api.LambdaFunctionHandlers` to platform-specific
- [ ] Test locally with your existing unit tests (should pass without changes!)
- [ ] Deploy to target platform
- [ ] Validate functionality
- [ ] Monitor performance and cold starts
- [ ] Celebrate multi-cloud deployment! 🎉

## Support

For questions or issues:
- 📖 [Documentation](../../README.md)
- 🐛 [Report Issues](https://github.com/exiton3/AppFactory/issues)
- 💬 [Discussions](https://github.com/exiton3/AppFactory/discussions)

---

**AppFactory v10.2.0** - Build once, deploy anywhere! 🌍
