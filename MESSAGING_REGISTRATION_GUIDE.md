# Unified Messaging - Registration Guide

## ✅ Correct Registration Patterns

### AWS SQS (NEW - Recommended)

```csharp
using AppFactory.Framework.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

// Startup.cs
public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // ✅ CORRECT - Register IMessageHandler from Messaging.Core
        services.AddScoped<IMessageHandler<OrderCreatedMessage>, OrderCreatedHandler>();
        
        // Business services
        services.AddScoped<IOrderService, OrderService>();
        services.AddSerilogLogging();
    }
}

// Handler (platform-agnostic)
public class OrderCreatedHandler : IMessageHandler<OrderCreatedMessage>
{
    public async Task HandleAsync(OrderCreatedMessage message, CancellationToken ct)
    {
        // Business logic
    }
}

// Lambda Function
public class OrderCreatedFunction : SqsMessageHandlerBase<OrderCreatedMessage>
{
    public OrderCreatedFunction() : base(new Startup()) { }
    protected override IStartup GetStartup() => new Startup();
    
    public Task<SQSBatchResponse> FunctionHandler(SQSEvent e, ILambdaContext c)
        => Handle(e, c);
}
```

### Azure Service Bus (NEW - Recommended)

```csharp
using AppFactory.Framework.Messaging.Abstractions;

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // ✅ CORRECT - Same IMessageHandler interface as AWS!
        services.AddScoped<IMessageHandler<OrderCreatedMessage>, OrderCreatedHandler>();
        
        services.AddScoped<IOrderService, OrderService>();
        services.AddSerilogLogging();
    }
}

// Handler - SAME as AWS! Platform-agnostic!
public class OrderCreatedHandler : IMessageHandler<OrderCreatedMessage>
{
    public async Task HandleAsync(OrderCreatedMessage message, CancellationToken ct)
    {
        // Same business logic works on AWS AND Azure!
    }
}

// Azure Function
public class OrderCreatedFunction : ServiceBusFunctionHandlerBase<OrderCreatedMessage>
{
    public OrderCreatedFunction() : base(new Startup()) { }
    protected override IStartup GetStartup() => new Startup();
    
    [Function("ProcessOrderCreated")]
    public async Task Run(
        [ServiceBusTrigger("order-queue", Connection = "ServiceBus")] 
        ServiceBusReceivedMessage msg, FunctionContext ctx)
        => await Handle(msg, ctx);
}
```

### Azure Storage Queue (NEW - Recommended)

```csharp
using AppFactory.Framework.Messaging.Abstractions;

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // ✅ CORRECT - Same interface everywhere!
        services.AddScoped<IMessageHandler<OrderCreatedMessage>, OrderCreatedHandler>();
        
        services.AddScoped<IOrderService, OrderService>();
    }
}

// Azure Function
public class OrderCreatedFunction : QueueStorageFunctionHandlerBase<OrderCreatedMessage>
{
    public OrderCreatedFunction() : base(new Startup()) { }
    protected override IStartup GetStartup() => new Startup();
    
    [Function("ProcessOrderCreated")]
    public async Task Run(
        [QueueTrigger("order-queue", Connection = "AzureWebJobsStorage")] 
        QueueMessage msg, FunctionContext ctx)
        => await Handle(msg, ctx);
}
```

### AWS Lambda (LEGACY - Existing Code Only)

```csharp
using AppFactory.Framework.Messaging.LambdaHandlers;

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // ⚠️ LEGACY - Only for existing code
        services.AddScoped<ILambdaMessageProcessor<OrderCreatedMessage>, OrderCreatedProcessor>();
        
        services.AddScoped<IOrderService, OrderService>();
    }
}

// Legacy Processor
public class OrderCreatedProcessor : ILambdaMessageProcessor<OrderCreatedMessage>
{
    public async Task Process(OrderCreatedMessage message)
    {
        // Legacy implementation
    }
}

// Legacy Function
public class OrderCreatedFunction : LambdaMessageHandlerBase2<OrderCreatedMessage>
{
    public OrderCreatedFunction() : base(new Startup()) { }
    protected override IStartup GetStartup() => new Startup();
}
```

## ❌ Common Mistakes

### Wrong: Using removed IMessageProcessor

```csharp
// ❌ ERROR - IMessageProcessor was removed!
services.AddScoped<IMessageProcessor<OrderMessage>, OrderHandler>();
```

**Fix:**
```csharp
// ✅ CORRECT - Use IMessageHandler from Messaging.Core
using AppFactory.Framework.Messaging.Abstractions;
services.AddScoped<IMessageHandler<OrderMessage>, OrderHandler>();
```

### Wrong: Mixing Message Classes

```csharp
// ❌ ERROR - Wrong message class
using AppFactory.Framework.Messaging.LambdaHandlers; // Legacy namespace

public class OrderMessage : Message  // This is the OLD Message class!
{
}
```

**Fix:**
```csharp
// ✅ CORRECT - Use Message from Messaging.Core
using AppFactory.Framework.Messaging.Abstractions;

public class OrderMessage : Message  // This is the NEW platform-agnostic Message class!
{
}
```

### Wrong: Using Source and Attributes properties

```csharp
// ❌ ERROR - Message from Messaging.Core doesn't have these
var message = new OrderMessage
{
    Source = "test",  // ERROR: Property doesn't exist
    Attributes = new Dictionary<string, string>()  // ERROR: Use Properties instead
};
```

**Fix:**
```csharp
// ✅ CORRECT - Use Properties and set Source as a property
var message = new OrderMessage
{
    MessageId = "test-123",
    Body = JsonSerializer.Serialize(orderData),
    Properties = new Dictionary<string, string>
    {
        ["Source"] = "test",
        ["CorrelationId"] = "corr-123"
    },
    EnqueuedTimeUtc = DateTime.UtcNow,
    DeliveryCount = 1
};
```

## 📊 Registration Matrix

| Platform | Interface | Message Class | Base Handler | Project |
|----------|-----------|---------------|--------------|---------|
| **AWS SQS (New)** ⭐ | `IMessageHandler<T>` | `Messaging.Core.Message` | `SqsMessageHandlerBase<T>` | Messaging.Aws |
| **Azure Service Bus** ⭐ | `IMessageHandler<T>` | `Messaging.Core.Message` | `ServiceBusFunctionHandlerBase<T>` | Messaging.Azure |
| **Azure Storage Queue** ⭐ | `IMessageHandler<T>` | `Messaging.Core.Message` | `QueueStorageFunctionHandlerBase<T>` | Messaging.Azure |
| **AWS Lambda (Legacy)** ⚠️ | `ILambdaMessageProcessor<T>` | `Messaging.LambdaHandlers.Message` | `LambdaMessageHandlerBase2<T>` | Messaging |

## ✅ Using Namespaces

### Correct Using Statements

```csharp
// For NEW implementations (AWS, Azure)
using AppFactory.Framework.Messaging.Abstractions;  // ← IMessageHandler, Message
using AppFactory.Framework.Messaging.Aws.Handlers;  // ← SqsMessageHandlerBase
using AppFactory.Framework.Messaging.Azure.FunctionHandlers;  // ← Service Bus/Storage handlers

// For LEGACY implementations only
using AppFactory.Framework.Messaging.LambdaHandlers;  // ← ILambdaMessageProcessor, legacy Message
```

## 🎯 Quick Checklist

### For New AWS SQS Handler
- [ ] Use `IMessageHandler<TMessage>` from `Messaging.Abstractions`
- [ ] Inherit message from `Message` in `Messaging.Abstractions`
- [ ] Implement `HandleAsync(message, ct)` method
- [ ] Register with `services.AddScoped<IMessageHandler<T>, Handler>()`
- [ ] Extend `SqsMessageHandlerBase<T>` for Lambda function
- [ ] Access metadata via `message.Properties["key"]`

### For New Azure Service Bus Handler
- [ ] Use `IMessageHandler<TMessage>` from `Messaging.Abstractions`
- [ ] Inherit message from `Message` in `Messaging.Abstractions`
- [ ] Implement `HandleAsync(message, ct)` method
- [ ] Register with `services.AddScoped<IMessageHandler<T>, Handler>()`
- [ ] Extend `ServiceBusFunctionHandlerBase<T>` for Azure Function
- [ ] Access metadata via `message.Properties["key"]`

### For New Azure Storage Queue Handler
- [ ] Use `IMessageHandler<TMessage>` from `Messaging.Abstractions`
- [ ] Inherit message from `Message` in `Messaging.Abstractions`
- [ ] Implement `HandleAsync(message, ct)` method
- [ ] Register with `services.AddScoped<IMessageHandler<T>, Handler>()`
- [ ] Extend `QueueStorageFunctionHandlerBase<T>` for Azure Function
- [ ] Access metadata via `message.Properties["key"]`

### For Legacy AWS Lambda (Existing Code)
- [ ] Use `ILambdaMessageProcessor<TMessage>` from `Messaging.LambdaHandlers`
- [ ] Inherit message from `Message` in `Messaging.LambdaHandlers`
- [ ] Implement `Process(message)` method (no cancellation token)
- [ ] Register with `services.AddScoped<ILambdaMessageProcessor<T>, Processor>()`
- [ ] Extend `LambdaMessageHandlerBase2<T>`
- [ ] Access metadata via `message.Attributes["key"]` and `message.Source`

## 🔄 Migration Steps

1. **Update Message Class**
   ```diff
   - using AppFactory.Framework.Messaging.LambdaHandlers;
   + using AppFactory.Framework.Messaging.Abstractions;
   
   public class OrderMessage : Message { }
   ```

2. **Update Handler Interface**
   ```diff
   - public class OrderHandler : ILambdaMessageProcessor<OrderMessage>
   + public class OrderHandler : IMessageHandler<OrderMessage>
   ```

3. **Update Method Signature**
   ```diff
   - public async Task Process(OrderMessage message)
   + public async Task HandleAsync(OrderMessage message, CancellationToken ct)
   ```

4. **Update Property Access**
   ```diff
   - var source = message.Source;
   + var source = message.Properties.GetValueOrDefault("Source");
   
   - var attr = message.Attributes["key"];
   + var attr = message.Properties.GetValueOrDefault("key");
   ```

5. **Update Registration**
   ```diff
   - services.AddScoped<ILambdaMessageProcessor<OrderMessage>, OrderHandler>();
   + services.AddScoped<IMessageHandler<OrderMessage>, OrderHandler>();
   ```

6. **Update Function Base Class**
   ```diff
   - using AppFactory.Framework.Messaging.LambdaHandlers;
   + using AppFactory.Framework.Messaging.Aws.Handlers;
   
   - public class Function : LambdaMessageHandlerBase2<OrderMessage>
   + public class Function : SqsMessageHandlerBase<OrderMessage>
   ```

## 📚 Related Documentation

- [Unified Architecture Overview](UNIFIED_MESSAGING_ARCHITECTURE_FINAL.md)
- [AWS SQS Handler Guide](src/AppFactory.Framework.Messaging.Aws/Handlers/README.md)
- [Azure Service Bus Handler Guide](src/AppFactory.Framework.Messaging.Azure/FunctionHandlers/README.md)
- [Messaging Core Abstractions](src/AppFactory.Framework.Messaging.Core/README.md)
