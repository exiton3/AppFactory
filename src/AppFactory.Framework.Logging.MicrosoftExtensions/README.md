# AppFactory.Framework.Logging.MicrosoftExtensions

Microsoft.Extensions.Logging (MEL) implementation of AppFactory Framework logging abstractions. Provides standard .NET logging integration.

## 📦 Installation

```bash
dotnet add package AppFactory.Framework.Logging.MicrosoftExtensions
```

This automatically includes `AppFactory.Framework.Logging.Abstractions` as a dependency.

## 🚀 Quick Start

### Basic Configuration

```csharp
using AppFactory.Framework.Logging.MicrosoftExtensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register Microsoft.Extensions.Logging-based logging
services.AddMicrosoftExtensionsLogging();

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger>();

logger.LogInformation("Application started");
```

### With ASP.NET Core

```csharp
using AppFactory.Framework.Logging.MicrosoftExtensions;

var builder = WebApplication.CreateBuilder(args);

// ASP.NET Core configures Microsoft.Extensions.Logging automatically
// Just add the AppFactory adapter
builder.Services.AddMicrosoftExtensionsLogging();

var app = builder.Build();

// Use ILogger in your services
public class UserController : ControllerBase
{
    private readonly ILogger _logger;

    public UserController(ILogger logger)
    {
        _logger = logger; // AppFactory.ILogger backed by MEL
    }

    [HttpGet]
    public IActionResult GetUsers()
    {
        _logger.LogInformation("Getting users");
        return Ok();
    }
}
```

### With Custom Configuration

```csharp
services.AddMicrosoftExtensionsLogging(builder =>
{
    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
    builder.AddConsole();
    builder.AddDebug();
    builder.AddEventLog();
    builder.AddApplicationInsights();
});
```

### With Minimum Log Level

```csharp
using AppFactory.Framework.Logging;

services.AddMicrosoftExtensionsLogging(LogLevel.Information);
```

### With Custom Category

```csharp
services.AddMicrosoftExtensionsLogging("MyApp.CustomCategory");
```

## 🔧 Features

### Structured Logging

Leverage Microsoft.Extensions.Logging structured logging:

```csharp
// In MEL configuration
builder.Services.AddLogging(logging =>
{
    logging.AddJsonConsole(); // Structured JSON output
});

builder.Services.AddMicrosoftExtensionsLogging();

// Use structured logging
logger.LogInformation("User {UserId} logged in from {IpAddress}", userId, ipAddress);
```

### Performance Tracking

Automatically track and log operation duration:

```csharp
using (logger.LogPerformance("DatabaseQuery"))
{
    // Your database operation
    // Logs: "Performance tracking completed: DatabaseQuery took 145ms"
}
```

### Multiple Providers

Configure multiple logging providers:

```csharp
services.AddMicrosoftExtensionsLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
    builder.AddEventLog();
    builder.AddApplicationInsights();
    builder.AddAzureWebAppDiagnostics();
});
```

## 🎨 Integration Examples

### Example 1: Console Application

```csharp
using AppFactory.Framework.Logging;
using AppFactory.Framework.Logging.MicrosoftExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddMicrosoftExtensionsLogging(builder =>
{
    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
    builder.AddConsole(options => options.IncludeScopes = true);
});

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger>();

logger.LogDebug("Debug message");
logger.LogInformation("Info message");
logger.LogWarning("Warning message");
logger.LogError("Error message");
```

### Example 2: ASP.NET Core API

```csharp
// Program.cs
using AppFactory.Framework.Logging.MicrosoftExtensions;

var builder = WebApplication.CreateBuilder(args);

// Configure MEL
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);

// Add AppFactory logging adapter
builder.Services.AddMicrosoftExtensionsLogging();

// Register your services
builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();
app.MapControllers();
app.Run();

// UserService.cs
public class UserService : IUserService
{
    private readonly ILogger _logger;

    public UserService(ILogger logger)
    {
        _logger = logger;
    }

    public async Task CreateUser(string email)
    {
        _logger.LogInformation("Creating user: {Email}", email);
        
        using (_logger.LogPerformance("CreateUser"))
        {
            // Create user logic
        }
    }
}
```

### Example 3: With Serilog as MEL Provider

```csharp
using Serilog;
using AppFactory.Framework.Logging.MicrosoftExtensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog as the MEL provider
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add AppFactory adapter (now backed by Serilog through MEL)
builder.Services.AddMicrosoftExtensionsLogging();

var app = builder.Build();
```

### Example 4: With Application Insights

```csharp
using Microsoft.ApplicationInsights;
using AppFactory.Framework.Logging.MicrosoftExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Configure MEL to use Application Insights
builder.Logging.AddApplicationInsights();

// Add AppFactory adapter
builder.Services.AddMicrosoftExtensionsLogging();

var app = builder.Build();
```

## 📊 Comparison with Serilog Package

| Feature | MicrosoftExtensions | Serilog |
|---------|---------------------|---------|
| .NET Standard | ✅ Native | ✅ Via Serilog |
| ASP.NET Core | ✅ Built-in | ✅ Via Provider |
| Configuration | ✅ appsettings.json | ✅ Code/Config |
| Structured Logging | ✅ Native | ✅ Excellent |
| AWS Lambda | ✅ Good | ✅ Excellent |
| Learning Curve | ✅ Low (Standard) | ⚠️ Medium |

## 🔌 When to Use

### Use MicrosoftExtensions When:
- ✅ Building ASP.NET Core applications
- ✅ Want standard .NET ecosystem alignment
- ✅ Using Azure Application Insights
- ✅ Team familiar with MEL
- ✅ Configuration-driven logging setup

### Use Serilog When:
- ✅ Building AWS Lambda functions
- ✅ Need advanced structured logging
- ✅ Want rich sink ecosystem
- ✅ Prefer fluent configuration API
- ✅ Need compact JSON output

## 📋 Configuration Options

### Minimum Log Level

```csharp
services.AddMicrosoftExtensionsLogging(LogLevel.Debug);
```

### Custom Category

```csharp
services.AddMicrosoftExtensionsLogging("MyApp");
```

### Full Control

```csharp
services.AddMicrosoftExtensionsLogging(builder =>
{
    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
    builder.AddFilter("System", Microsoft.Extensions.Logging.LogLevel.Warning);
    builder.AddConsole();
    builder.AddDebug();
});
```

## 🎯 Log Levels Mapping

| AppFactory | Microsoft.Extensions.Logging |
|------------|------------------------------|
| Trace | Trace |
| Debug | Debug |
| Information | Information |
| Warning | Warning |
| Error | Error |
| Critical | Critical |

## 🏗️ Architecture

This package acts as an **adapter** between AppFactory logging abstractions and Microsoft.Extensions.Logging:

```
┌─────────────────────────────────────────┐
│   Your Application Code                 │
│   (uses AppFactory.ILogger)             │
└────────────────┬────────────────────────┘
                 │ uses ↓
┌────────────────┴────────────────────────┐
│  AppFactory.Framework.Logging           │
│  .Abstractions                          │
└────────────────┬────────────────────────┘
                 ↑ implements
┌────────────────┴────────────────────────┐
│  AppFactory.Framework.Logging           │
│  .MicrosoftExtensions                   │
│  (Adapter)                              │
└────────────────┬────────────────────────┘
                 │ uses ↓
┌────────────────┴────────────────────────┐
│  Microsoft.Extensions.Logging           │
│  (Standard .NET Logging)                │
└────────────────┬────────────────────────┘
                 │ writes to ↓
┌────────────────┴────────────────────────┐
│  Providers (Console, Debug,             │
│  ApplicationInsights, etc.)             │
└─────────────────────────────────────────┘
```

## 🔗 Related Packages

- `AppFactory.Framework.Logging.Abstractions` - Core interfaces
- `AppFactory.Framework.Logging.Serilog` - Serilog implementation
- `AppFactory.Framework.Logging` - Legacy package (deprecated)

## 💡 Best Practices

1. **Use dependency injection** - Let DI resolve ILogger
2. **Structured logging** - Use message templates with parameters
3. **Performance tracking** - Use `LogPerformance()` for critical operations
4. **Log levels** - Use appropriate levels (Info for flow, Debug for details)
5. **Category names** - Use meaningful categories for filtering

## 🐛 Troubleshooting

### Logs not appearing

Check MEL is configured:
```csharp
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
```

### Double logging

If you see duplicate logs, ensure you're not registering AppFactory logging twice:
```csharp
// ❌ Don't do this
services.AddMicrosoftExtensionsLogging();
services.AddSerilogLogging(); // Conflict!

// ✅ Choose one
services.AddMicrosoftExtensionsLogging();
```

## 📄 License

Copyright © Sergey Kichuk. Licensed under the MIT License.

## 🔗 Links

- [GitHub Repository](https://github.com/exiton3/AppFactory)
- [Microsoft.Extensions.Logging Documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)
- [NuGet Package](https://www.nuget.org/packages/AppFactory.Framework.Logging.MicrosoftExtensions/)
