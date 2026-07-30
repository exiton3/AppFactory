# AppFactory Framework v10.2.0 - Logging Updates

## 🔄 Logging Architecture Redesign

The logging package has been split into **three focused packages** for better flexibility:

**📦 New Packages:**
- `AppFactory.Framework.Logging.Abstractions` - Pure interfaces (zero dependencies)
- `AppFactory.Framework.Logging.Serilog` - Serilog implementation (best for AWS Lambda)
- `AppFactory.Framework.Logging.MicrosoftExtensions` - Standard .NET logging (best for ASP.NET Core)

**✨ Choose Your Implementation:**
```csharp
// For ASP.NET Core / Azure
services.AddMicrosoftExtensionsLogging();

// For AWS Lambda / Advanced structured logging
services.AddSerilogLogging(config => config.LogLevel = LogLevel.Information);

// For libraries (abstractions only)
dotnet add package AppFactory.Framework.Logging.Abstractions
```

**🔄 Migration:** The original `AppFactory.Framework.Logging` package is maintained for backward compatibility - **no breaking changes**.

---

## Installation

```bash
# Choose ONE logging implementation:

# For ASP.NET Core applications
dotnet add package AppFactory.Framework.Logging.MicrosoftExtensions --version 10.2.0

# For AWS Lambda functions
dotnet add package AppFactory.Framework.Logging.Serilog --version 10.2.0

# For libraries (abstractions only)
dotnet add package AppFactory.Framework.Logging.Abstractions --version 10.2.0
```

## Documentation

- [Logging Abstractions README](https://github.com/exiton3/AppFactory/blob/master/src/AppFactory.Framework.Logging.Abstractions/README.md)
- [Serilog Implementation README](https://github.com/exiton3/AppFactory/blob/master/src/AppFactory.Framework.Logging.Serilog/README.md)
- [Microsoft.Extensions.Logging Implementation README](https://github.com/exiton3/AppFactory/blob/master/src/AppFactory.Framework.Logging.MicrosoftExtensions/README.md)
- [Logging Package Selection Guide](https://github.com/exiton3/AppFactory/blob/master/LOGGING_PACKAGE_SELECTION_GUIDE.md)
