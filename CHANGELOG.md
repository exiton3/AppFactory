# Changelog

All notable changes to the AppFactory Framework will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
