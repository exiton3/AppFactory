# Changelog

All notable changes to the AppFactory Framework will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
