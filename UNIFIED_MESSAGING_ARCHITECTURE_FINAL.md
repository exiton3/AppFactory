# Unified Messaging Architecture - Final Implementation

## ✅ Architecture Overview

We now have a **clean, unified messaging architecture** with proper separation of concerns:

```
┌──────────────────────────────────────────────────────────┐
│          Messaging.Core (Platform-Agnostic)              │
│                                                          │
│  • IMessageHandler<TMessage>  ← SINGLE SOURCE OF TRUTH   │
│  • Message class (with Properties, EnqueuedTimeUtc, etc) │
│  • IMessage interface                                    │
└────────────────────┬─────────────────────────────────────┘
                     │
                     │ Implements
        ┌────────────┼────────────┐
        │            │            │
┌───────▼─────┐  ┌──▼────────┐  ┌▼───────────────────────┐
│ Messaging   │  │ Messaging │  │ Messaging.Azure        │
│ .Aws        │  │ (Legacy)  │  │                        │
│             │  │ UNCHANGED │  │ ServiceBusFunction     │
│ SqsMessage  │  │           │  │ HandlerBase            │
│ HandlerBase │  │ Lambda    │  │                        │
│             │  │ Message   │  │ QueueStorageFunction   │
│ Uses:       │  │ Handler   │  │ HandlerBase            │
│ IMessage    │  │ Base2     │  │                        │
│ Handler     │  │           │  │ Uses: IMessageHandler  │
└─────────────┘  │ Uses:     │  └────────────────────────┘
                 │ ILambda   │
                 │ Message   │
                 │ Processor │
                 └───────────┘
```

## 🎯 Key Interfaces

### 1. IMessageHandler<TMessage> (Messaging.Core) ⭐ PRIMARY

```csharp
namespace AppFactory.Framework.Messaging.Abstractions;

/// <summary>
/// Platform-agnostic message handler - USE THIS FOR ALL NEW CODE
/// </summary>
public interface IMessageHandler<TMessage> where TMessage : class
{
    Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);
}
```

**Use For:**
- ✅ AWS SQS (new `SqsMessageHandlerBase`)
- ✅ Azure Service Bus (new `ServiceBusFunctionHandlerBase`)
- ✅ Azure Storage Queue (new `QueueStorageFunctionHandlerBase`)
- ✅ Any future message broker

### 2. ILambdaMessageProcessor<T> (Messaging - Legacy)

```csharp
namespace AppFactory.Framework.Messaging.LambdaHandlers;

/// <summary>
/// AWS Lambda-specific message processor (legacy).
/// For new code, use IMessageHandler from Messaging.Core.
/// </summary>
public interface ILambdaMessageProcessor<T> where T : LegacyMessage
{
    Task Process(T message);
}
```

**Use For:**
- ⚠️ Existing code in legacy `Messaging` project
- ⚠️ Backward compatibility only

## 📦 Message Classes

### 1. Message (Messaging.Core) ⭐ PRIMARY

```csharp
namespace AppFactory.Framework.Messaging.Abstractions;

public class Message : IMessage
{
    public string MessageId { get; set; }
    public string Body { get; set; }
    public IDictionary<string, string> Properties { get; set; }  // ← Metadata
    public DateTime EnqueuedTimeUtc { get; set; }
    public int DeliveryCount { get; set; }
}
```

**Features:**
- ✅ Platform-agnostic metadata via `Properties` dictionary
- ✅ Delivery count tracking
- ✅ Enqueued timestamp
- ✅ Works with all cloud providers

### 2. Message (Messaging.LambdaHandlers) - Legacy

```csharp
namespace AppFactory.Framework.Messaging.LambdaHandlers;

public class Message
{
    public string MessageId { get; set; }
    public string Body { get; set; }
    public string Source { get; set; }
    public Dictionary<string, string> Attributes { get; set; }  // ← Legacy
    public DateTime Timestamp { get; set; }
}
```

**Use Only With:**
- ⚠️ Legacy `LambdaMessageHandlerBase2` in Messaging project

## 🏗️ Handler Base Classes

### AWS SQS Handlers

#### 1. SqsMessageHandlerBase<TMessage> ⭐ NEW - RECOMMENDED

**Project**: `AppFactory.Framework.Messaging.Aws`  
**Interface**: `IMessageHandler<TMessage>` from Messaging.Core  
**Message Type**: `Message` from Messaging.Core

```csharp
using AppFactory.Framework.Messaging.Abstractions;
using AppFactory.Framework.Messaging.Aws.Handlers;

// Message Handler (platform-agnostic)
public class OrderHandler : IMessageHandler<OrderMessage>
{
    public async Task HandleAsync(OrderMessage message, CancellationToken ct)
    {
        // Access metadata
        var source = message.Properties.GetValueOrDefault("Source");
        var deliveryCount = message.DeliveryCount;
        
        // Business logic
        var data = JsonSerializer.Deserialize<OrderData>(message.Body);
        await _orderService.ProcessAsync(data, ct);
    }
}

// Lambda Function
public class OrderFunction : SqsMessageHandlerBase<OrderMessage>
{
    public OrderFunction() : base(new Startup()) { }
    protected override IStartup GetStartup() => new Startup();
}

// Registration
services.AddScoped<IMessageHandler<OrderMessage>, OrderHandler>();
```

#### 2. LambdaMessageHandlerBase2<TMessage> - LEGACY

**Project**: `AppFactory.Framework.Messaging`  
**Interface**: `ILambdaMessageProcessor<TMessage>` from Messaging  
**Message Type**: `Message` from Messaging.LambdaHandlers

```csharp
// Legacy - keep using if already implemented
public class OrderProcessor : ILambdaMessageProcessor<OrderMessage>
{
    public async Task Process(OrderMessage message)
    {
        var source = message.Source;
        var attrs = message.Attributes;
        // ...
    }
}
```

### Azure Handlers

#### 1. ServiceBusFunctionHandlerBase<TMessage> ⭐ NEW

**Project**: `AppFactory.Framework.Messaging.Azure`  
**Interface**: `IMessageHandler<TMessage>` from Messaging.Core  
**Message Type**: `Message` from Messaging.Core

```csharp
using AppFactory.Framework.Messaging.Abstractions;

public class OrderHandler : IMessageHandler<OrderMessage>
{
    public async Task HandleAsync(OrderMessage message, CancellationToken ct)
    {
        var deliveryCount = message.DeliveryCount;
        var enqueuedTime = message.EnqueuedTimeUtc;
        var source = message.Properties.GetValueOrDefault("Source");
        
        // Business logic
    }
}

public class OrderFunction : ServiceBusFunctionHandlerBase<OrderMessage>
{
    [Function("ProcessOrder")]
    public async Task Run(
        [ServiceBusTrigger("queue", Connection = "ServiceBus")] 
        ServiceBusReceivedMessage msg, FunctionContext ctx)
        => await Handle(msg, ctx);
}

services.AddScoped<IMessageHandler<OrderMessage>, OrderHandler>();
```

#### 2. QueueStorageFunctionHandlerBase<TMessage> ⭐ NEW

**Project**: `AppFactory.Framework.Messaging.Azure`  
**Interface**: `IMessageHandler<TMessage>` from Messaging.Core  
**Message Type**: `Message` from Messaging.Core

```csharp
public class OrderFunction : QueueStorageFunctionHandlerBase<OrderMessage>
{
    [Function("ProcessOrder")]
    public async Task Run(
        [QueueTrigger("queue", Connection = "AzureWebJobsStorage")] 
        QueueMessage msg, FunctionContext ctx)
        => await Handle(msg, ctx);
}
```

## 📊 Metadata Access Patterns

### Platform-Agnostic Message (Messaging.Core)

```csharp
public async Task HandleAsync(OrderMessage message, CancellationToken ct)
{
    // Standard properties
    var messageId = message.MessageId;
    var body = message.Body;
    var deliveryCount = message.DeliveryCount;
    var enqueuedTime = message.EnqueuedTimeUtc;
    
    // Platform-specific metadata via Properties
    var source = message.Properties.GetValueOrDefault("Source");
    var correlationId = message.Properties.GetValueOrDefault("CorrelationId");
    
    // AWS SQS specific
    var eventSource = message.Properties.GetValueOrDefault("EventSource");
    var queueArn = message.Properties.GetValueOrDefault("EventSourceARN");
    var sqsReceiveCount = message.Properties.GetValueOrDefault("SQS_ApproximateReceiveCount");
    
    // Azure Service Bus specific
    var sessionId = message.Properties.GetValueOrDefault("SessionId");
    var sequenceNumber = message.Properties.GetValueOrDefault("SequenceNumber");
    var subject = message.Properties.GetValueOrDefault("Subject");
    
    // Azure Storage Queue specific
    var dequeueCount = message.Properties.GetValueOrDefault("DequeueCount");
    var insertedOn = message.Properties.GetValueOrDefault("InsertedOn");
}
```

### Legacy Message (Messaging.LambdaHandlers)

```csharp
public async Task Process(OrderMessage message)
{
    var messageId = message.MessageId;
    var body = message.Body;
    var source = message.Source;
    var timestamp = message.Timestamp;
    
    // Custom attributes
    var correlationId = message.Attributes.GetValueOrDefault("CorrelationId");
}
```

## 🔄 Migration Guide

### From LambdaMessageHandlerBase2 to SqsMessageHandlerBase

```diff
// Handler
- using AppFactory.Framework.Messaging.LambdaHandlers;
+ using AppFactory.Framework.Messaging.Abstractions;

- public class OrderProcessor : ILambdaMessageProcessor<OrderMessage>
+ public class OrderHandler : IMessageHandler<OrderMessage>
{
-   public async Task Process(OrderMessage message)
+   public async Task HandleAsync(OrderMessage message, CancellationToken ct)
    {
-       var source = message.Source;
+       var source = message.Properties.GetValueOrDefault("Source");
        
-       var correlationId = message.Attributes.GetValueOrDefault("CorrelationId");
+       var correlationId = message.Properties.GetValueOrDefault("CorrelationId");
        
+       var deliveryCount = message.DeliveryCount;
+       var enqueuedTime = message.EnqueuedTimeUtc;
        
        // Business logic
    }
}

// Lambda Function
- using AppFactory.Framework.Messaging.LambdaHandlers;
+ using AppFactory.Framework.Messaging.Aws.Handlers;

- public class Function : LambdaMessageHandlerBase2<OrderMessage>
+ public class Function : SqsMessageHandlerBase<OrderMessage>

// Registration
- services.AddScoped<ILambdaMessageProcessor<OrderMessage>, OrderProcessor>();
+ services.AddScoped<IMessageHandler<OrderMessage>, OrderHandler>();

// Message Definition
+ using AppFactory.Framework.Messaging.Abstractions;

- public class OrderMessage : AppFactory.Framework.Messaging.LambdaHandlers.Message
+ public class OrderMessage : Message  // From Messaging.Core
```

## ✅ Benefits of Unified Architecture

### 1. Single Source of Truth
- **One interface**: `IMessageHandler<TMessage>` for all new code
- **One message class**: `Message` from Messaging.Core
- **Consistency**: Same patterns across AWS, Azure, and future clouds

### 2. Platform-Agnostic Handlers
```csharp
// ✅ This handler works with AWS SQS, Azure Service Bus, AND Azure Storage Queue!
public class OrderHandler : IMessageHandler<OrderMessage>
{
    public async Task HandleAsync(OrderMessage message, CancellationToken ct)
    {
        // Platform-agnostic business logic
        var data = JsonSerializer.Deserialize<OrderData>(message.Body);
        await _orderService.ProcessAsync(data, ct);
    }
}
```

### 3. Easy Testing
```csharp
[Fact]
public async Task HandleAsync_ValidMessage_ProcessesOrder()
{
    // No cloud dependencies!
    var message = new OrderMessage
    {
        MessageId = "test-123",
        Body = JsonSerializer.Serialize(new OrderData { OrderId = "123" }),
        Properties = new Dictionary<string, string> { ["Source"] = "test" },
        DeliveryCount = 1,
        EnqueuedTimeUtc = DateTime.UtcNow
    };
    
    var handler = new OrderHandler(_mockService, _mockLogger);
    await handler.HandleAsync(message, CancellationToken.None);
    
    // Assert
}
```

### 4. Rich Metadata
- ✅ Delivery count tracking
- ✅ Enqueued timestamp
- ✅ Platform-specific attributes via Properties
- ✅ Correlation/causation IDs

### 5. Backward Compatibility
- ✅ Legacy `Messaging` project unchanged
- ✅ Existing `ILambdaMessageProcessor` still works
- ✅ Old `LambdaMessageHandlerBase2` still functional

## 📚 Project Structure

```
AppFactory.Framework.Messaging.Core/
├── Abstractions/
│   ├── IMessageHandler.cs           ⭐ Platform-agnostic interface
│   ├── IMessage.cs
│   └── Message.cs                   ⭐ Platform-agnostic message class

AppFactory.Framework.Messaging.Aws/
├── Handlers/
│   ├── SqsMessageHandlerBase.cs    ⭐ NEW - Uses IMessageHandler
│   ├── LambdaMessageHandlerBase.cs  (with context support)
│   └── README.md

AppFactory.Framework.Messaging.Azure/
├── FunctionHandlers/
│   ├── ServiceBusFunctionHandlerBase.cs  ⭐ NEW - Uses IMessageHandler
│   ├── QueueStorageFunctionHandlerBase.cs ⭐ NEW - Uses IMessageHandler
│   └── README.md

AppFactory.Framework.Messaging/       (UNCHANGED - Legacy)
├── LambdaHandlers/
│   ├── LambdaMessageHandlerBase2.cs
│   ├── ILambdaMessageProcessor.cs
│   └── Message.cs                    (legacy message class)
```

## 🎯 Recommendation

### ✅ For New Code
Use:
- **Interface**: `IMessageHandler<TMessage>` from `Messaging.Core`
- **Message**: `Message` from `Messaging.Core`
- **AWS**: `SqsMessageHandlerBase<T>` from `Messaging.Aws`
- **Azure Service Bus**: `ServiceBusFunctionHandlerBase<T>` from `Messaging.Azure`
- **Azure Storage**: `QueueStorageFunctionHandlerBase<T>` from `Messaging.Azure`

### ⚠️ For Existing Code
Keep using:
- `ILambdaMessageProcessor<T>` from `Messaging`
- `LambdaMessageHandlerBase2<T>` from `Messaging`
- Migrate when convenient

## ✅ Build Status

**All projects compile successfully!**

```
✅ AppFactory.Framework.Messaging.Core
✅ AppFactory.Framework.Messaging.Aws
✅ AppFactory.Framework.Messaging.Azure
✅ AppFactory.Framework.Messaging (unchanged)
```

**0 Build Errors | Clean Architecture | Single Source of Truth**
