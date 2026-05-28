# AppFactory.Framework.Logging.Abstractions

Core logging abstractions for the AppFactory Framework. This package provides interfaces and contracts for logging without any implementation dependencies.

## 📦 Installation

```bash
dotnet add package AppFactory.Framework.Logging.Abstractions
```

## 🎯 Purpose

This package contains only the logging abstractions (`ILogger`, `ILoggerFactory`, etc.) without any specific logging implementation. This allows you to:

- Write code against stable interfaces
- Choose your logging implementation (Serilog, Microsoft.Extensions.Logging, etc.)
- Avoid taking dependencies on specific logging frameworks in your domain/application layers

## 🔌 Implementation Packages

Choose ONE of the following implementation packages:

### Serilog Implementation (Recommended for AWS Lambda)
```bash
dotnet add package AppFactory.Framework.Logging.Serilog
```

### Microsoft.Extensions.Logging Implementation (Standard .NET)
```bash
dotnet add package AppFactory.Framework.Logging.MicrosoftExtensions
```

## 📚 Interfaces

### ILogger

Core logging interface with methods for different log levels:

```csharp
public interface ILogger
{
    void LogTrace(string message);
    void LogDebug(string message);
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(string message);
    void LogError(Exception exception, string message);
    void LogCritical(string message);
    IDisposable LogPerformance(string operationName);
}
```

### ILoggerFactory

Factory for creating logger instances:

```csharp
public interface ILoggerFactory
{
    ILogger CreateLogger(string categoryName);
    ILogger CreateLogger<T>();
}
```

### LogLevel

Enumeration of log levels:

```csharp
public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical
}
```

## 🔧 Usage

### In Your Domain/Application Layer

Reference only the abstractions package:

```csharp
using AppFactory.Framework.Logging;

public class UserService
{
    private readonly ILogger _logger;

    public UserService(ILogger logger)
    {
        _logger = logger;
    }

    public void CreateUser(string email)
    {
        _logger.LogInformation($"Creating user: {email}");
        
        using (_logger.LogPerformance("CreateUser"))
        {
            // Your logic here
        }
    }
}
```

### Performance Logging

Track operation duration:

```csharp
using (_logger.LogPerformance("DatabaseQuery"))
{
    // Your operation here
    // Automatically logs duration when disposed
}
```

## 🏗️ Architecture

This package follows the **Dependency Inversion Principle**:

```
┌─────────────────────────────────────────┐
│   Your Application/Domain Layer         │
│   (depends on abstractions only)        │
└────────────────┬────────────────────────┘
                 │ depends on ↓
┌────────────────┴────────────────────────┐
│  AppFactory.Framework.Logging           │
│  .Abstractions                          │
│  (interfaces only)                      │
└─────────────────────────────────────────┘
                 ↑ implemented by
┌────────────────┴────────────────────────┐
│  Implementation Packages                 │
│  - Logging.Serilog                      │
│  - Logging.MicrosoftExtensions          │
└─────────────────────────────────────────┘
```

## 📄 License

Copyright © Sergey Kichuk. Licensed under the MIT License.
