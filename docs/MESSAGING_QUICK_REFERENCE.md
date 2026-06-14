# Multi-Cloud Messaging - Quick Reference

## 🎯 One Processor, Multiple Clouds

```
Your Processor (Write Once)
        ↓
IMessageProcessor<TMessage>
        ↓
    ┌───┴───┬────────┐
    ↓       ↓        ↓
  AWS    Azure    Azure
Lambda  Service  Storage
 SQS     Bus     Queue
```

## 📝 Quick Start (3 Steps)

### 1. Create Processor
```csharp
public class OrderProcessor : IMessageProcessor<OrderMessage>
{
    public async Task Process(OrderMessage message)
    {
        // Your business logic here
    }
}
```

### 2. Register in Startup
```csharp
public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IMessageProcessor<OrderMessage>, OrderProcessor>();
    }
}
```

### 3. Choose Your Cloud

#### AWS Lambda
```csharp
public class Handler : LambdaMessageHandlerBase2<OrderMessage>
{
    public Handler() : base(new Startup()) { }
    protected override IStartup GetStartup() => new Startup();
    
    public Task<SQSBatchResponse> FunctionHandler(SQSEvent e, ILambdaContext c)
        => Handle(e, c);
}
```

#### Azure Service Bus Queue
```csharp
public class Handler : ServiceBusFunctionHandlerBase<OrderMessage>
{
    public Handler() : base(new Startup()) { }
    protected override IStartup GetStartup() => new Startup();
    
    [Function("ProcessOrder")]
    public Task Run(
        [ServiceBusTrigger("queue", Connection = "ServiceBus")] 
        ServiceBusReceivedMessage msg, FunctionContext ctx)
        => Handle(msg, ctx);
}
```

#### Azure Service Bus Topic
```csharp
[ServiceBusTrigger("topic", "subscription", Connection = "ServiceBus")]
=> HandleTopicMessage(msg, ctx);
```

#### Azure Storage Queue
```csharp
public class Handler : QueueStorageFunctionHandlerBase<OrderMessage>
{
    public Handler() : base(new Startup()) { }
    protected override IStartup GetStartup() => new Startup();
    
    [Function("ProcessOrder")]
    public Task Run(
        [QueueTrigger("queue", Connection = "AzureWebJobsStorage")] 
        QueueMessage msg, FunctionContext ctx)
        => Handle(msg, ctx);
}
```

## 📦 Message Structure

```csharp
public class OrderMessage : Message
{
    // Inherits:
    // - string Body
    // - string MessageId
    // - string Source
    // - Dictionary<string, string> Attributes
}
```

## 🔑 Access Metadata

```csharp
public async Task Process(OrderMessage message)
{
    // Common across ALL platforms
    var correlationId = message.Attributes.GetValueOrDefault("CorrelationId");
    var deliveryCount = message.Attributes.GetValueOrDefault("DeliveryCount");
    var enqueuedTime = message.Attributes.GetValueOrDefault("EnqueuedTimeUtc");
    
    // Platform-specific
    var sessionId = message.Attributes.GetValueOrDefault("SessionId"); // Service Bus
    var sequenceNum = message.Attributes.GetValueOrDefault("SequenceNumber"); // Service Bus
}
```

## 🏗️ Base Classes

| Platform | Base Class | Methods |
|----------|-----------|---------|
| AWS Lambda | `LambdaMessageHandlerBase2<T>` | `Handle()` |
| Service Bus Queue | `ServiceBusFunctionHandlerBase<T>` | `Handle()` |
| Service Bus Topic | `ServiceBusFunctionHandlerBase<T>` | `HandleTopicMessage()` |
| Storage Queue | `QueueStorageFunctionHandlerBase<T>` | `Handle()` |

All support: **Batch processing, DI, logging, error handling**

## 🔄 Processor Resolution

Automatically tries both interfaces:
1. `IMessageProcessor<T>` ← **Recommended**
2. `ILambdaMessageProcessor<T>` ← Backward compatible

## ✅ Checklist

- [ ] Define message type inheriting from `Message`
- [ ] Implement `IMessageProcessor<TMessage>`
- [ ] Register processor in `Startup.ConfigureServices()`
- [ ] Create handler extending appropriate base class
- [ ] Pass `Startup` to base class constructor
- [ ] Implement `GetStartup()` abstract method
- [ ] Add Function attribute (Azure) or entry point (AWS)
- [ ] Configure queue/topic connection

## 📊 Feature Support

|  | AWS Lambda | Service Bus | Storage Queue |
|---|-----------|-------------|---------------|
| Dead Letter Queue | ✅ | ✅ | ✅ |
| Retry Logic | ✅ | ✅ | ✅ |
| Batch Processing | ✅ | ✅ | ✅ |
| Sessions | ❌ | ✅ | ❌ |
| Pub/Sub (Topics) | ❌ | ✅ | ❌ |
| FIFO Ordering | ⚠️ FIFO queue | ✅ | ❌ |

## 🚀 Deploy

### AWS
```bash
serverless deploy --stage dev
```

### Azure
```bash
func azure functionapp publish <app-name>
```

## 📚 Full Documentation

- `UNIFIED_MULTI_CLOUD_MESSAGING_ARCHITECTURE.md` - Complete guide
- `MULTI_CLOUD_MESSAGING_SUMMARY.md` - Implementation details
- `src\AppFactory.Framework.Messaging\MessageProcessors\README.md` - Processors
