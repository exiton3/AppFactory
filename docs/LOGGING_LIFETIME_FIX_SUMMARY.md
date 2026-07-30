# Logging Lifetime Fix - Captive Dependency Resolution

## 🚨 Problem Identified

### **Captive Dependency Violation**

```
Singleton Service (CosmosDbClientFactory)
    ↓ captures
Scoped Service (ILogger)
    ↓
❌ VIOLATION: Singleton cannot capture scoped dependency
```

### **Runtime Error:**

```
System.InvalidOperationException: 
Cannot consume scoped service 'AppFactory.Framework.Logging.ILogger' 
from singleton 'AppFactory.Framework.DataAccess.CosmosDB.CosmosDb.ICosmosDbClientFactory'.
```

---

## 📋 Analysis

### **Problematic Registrations:**

#### **Microsoft.Extensions Logging (BEFORE FIX):**
```csharp
// src/AppFactory.Framework.Logging.MicrosoftExtensions/DependencyInjectionExtensions.cs
public static IServiceCollection AddMicrosoftExtensionsLogging(this IServiceCollection services)
{
    services.AddScoped<ILogger>(...);  // ❌ Scoped
}
```

#### **CosmosDB Registration:**
```csharp
// src/AppFactory.Framework.DataAccess.CosmosDB/DependencyRegistrationExtensions.cs
public static void RegisterCosmosDbPersistence(this IServiceCollection services)
{
    services.AddSingleton<ICosmosDbClientFactory, CosmosDbClientFactory>();
    //                                             ↑
    //                    Constructor: ILogger logger  ❌ Captures scoped!
}
```

### **Dependency Graph:**

```
Application Start
    ↓
Singleton: CosmosDbClientFactory (lives forever)
    ↓ captures reference
Scoped: ILogger (should be created per request)
    ↓
❌ Logger instance from first request is "captured"
   and reused for all subsequent requests
```

---

## ✅ Solution Implemented

### **Changed ILogger Lifetime to Singleton**

**Rationale:**
- Loggers are **stateless** and **thread-safe**
- Microsoft.Extensions.Logging's `ILogger<T>` is designed to be singleton
- No per-request state needed for logging
- Safe to share across requests

### **Files Changed:**

#### **1. AppFactory.Framework.Logging.MicrosoftExtensions/DependencyInjectionExtensions.cs**

```csharp
// BEFORE (❌ Scoped):
services.AddScoped<ILogger>(provider => ...);

// AFTER (✅ Singleton):
services.AddSingleton<ILogger>(provider =>
{
    var loggerFactory = provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
    var melLogger = loggerFactory.CreateLogger("AppFactory");
    return new MicrosoftExtensionsLogger(melLogger);
});
```

**Changed in 4 overloads:**
1. ✅ `AddMicrosoftExtensionsLogging()`
2. ✅ `AddMicrosoftExtensionsLogging(string categoryName)`
3. ✅ `AddMicrosoftExtensionsLogging(Action<ILoggingBuilder> configure)`
4. ✅ `AddMicrosoftExtensionsLogging(LogLevel minLogLevel)`

---

## 📊 Comparison: Serilog vs Microsoft.Extensions

| Package | Before | After | Already Correct? |
|---------|--------|-------|------------------|
| **Serilog** | Singleton ✅ | Singleton ✅ | Yes (no change needed) |
| **Microsoft.Extensions** | Scoped ❌ | Singleton ✅ | Fixed |

### **Serilog (No Change Needed):**

```csharp
// src/AppFactory.Framework.Logging.Serilog/DependencyInjectionExtension.cs
services.AddSingleton<ILogger>(provider =>
{
    var serilogLogger = provider.GetRequiredService<global::Serilog.ILogger>();
    return new SerilogLogger(serilogLogger);
});
```
✅ Already singleton - no issue!

---

## 🔍 Why Singleton is Safe for Loggers

### **Logger Characteristics:**

| Aspect | Details |
|--------|---------|
| **State** | Stateless - no mutable state |
| **Thread Safety** | Thread-safe by design |
| **Scope Context** | Captured at log time via scopes (MEL) or context enrichment (Serilog) |
| **Performance** | Better - no per-request allocation |
| **Lifetime** | Application lifetime |

### **Microsoft.Extensions.Logging Pattern:**

```csharp
// ILogger<T> is designed to be singleton:
public class MyService
{
    private readonly ILogger<MyService> _logger;  // Singleton injection
    
    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;  // Captured once, reused
    }
    
    public void DoWork()
    {
        using (_logger.BeginScope(new { RequestId = Guid.NewGuid() }))
        {
            _logger.LogInformation("Working...");  // Scope is per-invocation
        }
    }
}
```

**Key Point:** Request-specific context is added via **scopes**, not service lifetime.

---

## 🧪 Testing

### **Verification Steps:**

1. ✅ Build successful
2. ✅ No DI lifetime warnings
3. ✅ CosmosDB components can be resolved
4. ✅ Logging works in both singleton and scoped services

### **Test Scenarios:**

```csharp
// Scenario 1: Singleton captures logger (now works)
services.AddSingleton<ICosmosDbClientFactory, CosmosDbClientFactory>();

// Scenario 2: Scoped service uses logger (still works)
services.AddScoped<IRepository<User>, UserRepository>();

// Scenario 3: Transient service uses logger (still works)
services.AddTransient<IMyService, MyService>();
```

---

## 📈 Benefits

| Benefit | Description |
|---------|-------------|
| **No Runtime Errors** | Eliminates captive dependency exception |
| **Better Performance** | Single logger instance vs per-request |
| **Consistent with MEL** | Matches Microsoft.Extensions.Logging pattern |
| **Thread Safe** | Logger designed for concurrent access |
| **Simple DI Graph** | Less complex lifetime management |

---

## 🔧 Impact on Existing Code

### **No Breaking Changes:**

- ✅ Existing code continues to work
- ✅ Logger can still be injected into any service
- ✅ Request scopes still work via `ILogger.BeginScope()`
- ✅ Backward compatible

### **Migration Notes:**

If you were relying on scoped logger behavior:
- **Before:** Each request got a new logger instance
- **After:** Same logger instance, use scopes for request context

**Example:**
```csharp
// Instead of relying on logger instance lifetime:
public void Handle(Request request)
{
    _logger.LogInformation("Request {Id}", request.Id);  // ❌ No automatic context
}

// Use scopes for request context:
public void Handle(Request request)
{
    using (_logger.BeginScope(new { RequestId = request.Id }))
    {
        _logger.LogInformation("Handling request");  // ✅ Context included
    }
}
```

---

## 📚 References

- [Microsoft Docs: Dependency Injection Best Practices](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)
- [Captive Dependencies Explained](https://blog.ploeh.dk/2014/06/02/captive-dependency/)
- [ILogger<T> Design Rationale](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)

---

## ✅ Summary

**Problem:** Singleton `CosmosDbClientFactory` captured scoped `ILogger`  
**Solution:** Changed `ILogger` registration to singleton  
**Rationale:** Loggers are stateless, thread-safe, and designed for singleton lifetime  
**Result:** No captive dependency violation, better performance, consistent with MEL patterns  

**Build Status:** ✅ Successful  
**Tests:** ✅ All passing  
**Breaking Changes:** ❌ None  
