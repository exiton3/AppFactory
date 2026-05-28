# AppFactory.Framework.Logging.Serilog

Serilog implementation of AppFactory Framework logging abstractions. Perfect for AWS Lambda and serverless applications.

## 📦 Installation

```bash
# Install abstractions and Serilog implementation
dotnet add package AppFactory.Framework.Logging.Serilog
```

This will automatically include `AppFactory.Framework.Logging.Abstractions` as a dependency.

## 🚀 Quick Start

### Basic Configuration

```csharp
using AppFactory.Framework.Logging.Serilog;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register Serilog logging
services.AddSerilogLogging(config =>
{
    config.LogLevel = LogLevel.Information;
});

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger>();

logger.LogInformation("Application started");
```

### Configuration from Environment

```csharp
// Reads log level from "log_level" environment variable
services.AddSerilogLogging();
```

### Custom Serilog Configuration

```csharp
services.AddSerilogLogging(
    config => config.LogLevel = LogLevel.Debug,
    serilogConfig => serilogConfig
        .WriteTo.Console()
        .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
        .Enrich.WithProperty("Application", "MyApp")
);
```

## 🔧 Features

### Structured Logging

Serilog provides structured logging out of the box:

```csharp
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

### Multiple Sinks

Configure multiple output targets:

```csharp
services.AddSerilogLogging(
    config => config.LogLevel = LogLevel.Information,
    serilogConfig => serilogConfig
        .WriteTo.Console(new RenderedCompactJsonFormatter())
        .WriteTo.File("logs/app.log")
        .WriteTo.Seq("http://localhost:5341")
);
```

## 📋 Configuration Options

### LogConfig

```csharp
public class LogConfig
{
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}
```

### Log Levels

- `Trace` - Most detailed (Serilog: Verbose)
- `Debug` - Debugging information (Serilog: Debug)
- `Information` - General information (Serilog: Information)
- `Warning` - Warning messages (Serilog: Warning)
- `Error` - Error events (Serilog: Error)
- `Critical` - Critical failures (Serilog: Fatal)

## ☁️ AWS Lambda Integration

Perfect for AWS Lambda functions with structured JSON logging:

```csharp
public class Function
{
    private readonly ILogger _logger;

    public Function()
    {
        var services = new ServiceCollection();
        services.AddSerilogLogging(config =>
        {
            config.LogLevel = LogLevel.Information;
        });
        
        var serviceProvider = services.BuildServiceProvider();
        _logger = serviceProvider.GetRequiredService<ILogger>();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request, 
        ILambdaContext context)
    {
        using (_logger.LogPerformance("LambdaExecution"))
        {
            _logger.LogInformation("Processing request for path: {Path}", request.Path);
            
            // Your logic here
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Success"
            };
        }
    }
}
```

## 🎨 Output Formats

### Compact JSON (Default for Lambda)

```csharp
services.AddSerilogLogging(
    config => config.LogLevel = LogLevel.Information,
    serilogConfig => serilogConfig
        .WriteTo.Console(new RenderedCompactJsonFormatter())
);
```

### Plain Text

```csharp
services.AddSerilogLogging(
    config => config.LogLevel = LogLevel.Information,
    serilogConfig => serilogConfig
        .WriteTo.Console(new PlainTextFormatter())
);
```

## 📊 Comparison with Other Options

| Feature | Serilog | Microsoft.Extensions.Logging |
|---------|---------|------------------------------|
| Structured Logging | ✅ Native | ⚠️ Requires providers |
| AWS Lambda | ✅ Excellent | ✅ Good |
| Performance | ✅ High | ✅ High |
| Ecosystem | ✅ Large | ✅ Very Large |
| Configuration | ✅ Fluent API | ⚠️ Configuration-based |

## 🔗 Related Packages

- `AppFactory.Framework.Logging.Abstractions` - Core interfaces
- `AppFactory.Framework.Logging.MicrosoftExtensions` - MEL implementation (coming soon)
- `AppFactory.Framework.Logging` - Legacy package (deprecated)

## 📚 Examples

### Example 1: Console Application

```csharp
using AppFactory.Framework.Logging;
using AppFactory.Framework.Logging.Serilog;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddSerilogLogging(config => config.LogLevel = LogLevel.Debug);

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger>();

logger.LogDebug("Debug message");
logger.LogInformation("Info message");
logger.LogWarning("Warning message");
logger.LogError("Error message");
```

### Example 2: With Dependency Injection

```csharp
public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSerilogLogging(config =>
        {
            config.LogLevel = LogLevel.Information;
        });
        
        services.AddScoped<IUserService, UserService>();
    }
}

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

## 🐛 Troubleshooting

### Logs not appearing

Check your log level configuration:
```csharp
services.AddSerilogLogging(config =>
{
    config.LogLevel = LogLevel.Trace; // Most verbose
});
```

### JSON output in Lambda

Ensure you're using the compact JSON formatter:
```csharp
serilogConfig.WriteTo.Console(new RenderedCompactJsonFormatter())
```

## 📄 License

Copyright © Sergey Kichuk. Licensed under the MIT License.

## 🔗 Links

- [GitHub Repository](https://github.com/exiton3/AppFactory)
- [Serilog Documentation](https://serilog.net/)
- [NuGet Package](https://www.nuget.org/packages/AppFactory.Framework.Logging.Serilog/)
