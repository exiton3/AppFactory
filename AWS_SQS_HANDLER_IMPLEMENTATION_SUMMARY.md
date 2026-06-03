# AWS SQS Message Handler - Implementation Summary

## ✅ What Was Created

A **platform-agnostic AWS SQS message handler** following the **Publisher-Subscriber pattern** for reactive microservices.

## 🎯 Architecture Overview

```
┌───────────────────────────────────────────────────────────────┐
│                   Messaging.Core                              │
│           (Platform-Agnostic Abstractions)                    │
│                                                               │
│  - IMessageHandler<TMessage>                                  │
│  - IMessage interface                                         │
│  - Message class (concrete implementation)                    │
└──────────────────────┬────────────────────────────────────────┘
                       │
                       │ Used by
          ┌────────────▼───────────────┐
          │    Messaging.Aws           │
          │  (AWS SQS Integration)     │
          │                            │
          │ - SqsMessageHandlerBase    │
          │ - Maps SQS → Message       │
          │ - Batch failure handling   │
          │ - DI scope per message     │
          └────────────┬───────────────┘
                       │
                       │ Processes
          ┌────────────▼───────────────┐
          │     Amazon SQS Queue        │
          │   - FIFO or Standard        │
          │   - Dead Letter Queue       │
          │   - Batch processing        │
          └─────────────────────────────┘
```

## 📁 Files Created

### 1. **SqsMessageHandlerBase.cs**
**Location**: `src\AppFactory.Framework.Messaging.Aws\Handlers\SqsMessageHandlerBase.cs`

**Purpose**: AWS Lambda base class for SQS message processing with DI support

**Key Features**:
- ✅ Platform-agnostic `IMessageHandler<TMessage>` interface
- ✅ Automatic SQS → `Message` mapping with rich metadata
- ✅ Batch failure handling (returns failed message IDs)
- ✅ DI scope per message
- ✅ Performance logging wrapper
- ✅ Cancellation token support (respects Lambda timeout)
- ✅ Comprehensive error handling

**Usage Pattern**:
```csharp
public class OrderFunction : SqsMessageHandlerBase<OrderMessage>
{
    public OrderFunction() : base(new Startup()) { }
    protected override IStartup GetStartup() => new Startup();
    
    public Task<SQSBatchResponse> FunctionHandler(SQSEvent e, ILambdaContext c)
        => Handle(e, c);
}
```

### 2. **README.md**
**Location**: `src\AppFactory.Framework.Messaging.Aws\Handlers\README.md`

**Content**:
- Complete architecture documentation
- Usage examples with all patterns
- Message metadata mapping table
- Advanced scenarios (idempotency, retry, FIFO)
- Testing strategies (unit + integration)
- Error handling best practices
- Performance optimization tips
- Migration guide from LambdaMessageHandlerBase2

### 3. **EXAMPLES.md**
**Location**: `src\AppFactory.Framework.Messaging.Aws\Handlers\EXAMPLES.md`

**Content**:
- 7 complete working examples:
  1. Simple order processing
  2. Event-driven saga with compensation
  3. Dead Letter Queue processing
  4. FIFO queue with deduplication
  5. Batch processing with custom retry
  6. Message enrichment and transformation
  7. Multi-step workflow with state machine
- Complete serverless.yml configurations
- Testing examples

## 🔑 Key Design Decisions

### 1. Platform-Agnostic Handler Interface

**Decision**: Use `IMessageHandler<TMessage>` from `Messaging.Core`

**Rationale**:
- ✅ Business logic is cloud-agnostic
- ✅ Same handler can work with different message brokers
- ✅ Easier testing (no AWS dependencies in business logic)
- ✅ Follows separation of concerns

```csharp
// ✅ Platform-agnostic - can work with SQS, Service Bus, RabbitMQ, etc.
public class OrderHandler : IMessageHandler<OrderMessage>
{
    public async Task HandleAsync(OrderMessage message, CancellationToken ct)
    {
        // Business logic here
    }
}
```

### 2. Message Class Instead of IMessage Interface

**Decision**: Use concrete `Message` class as constraint (`where TMessage : Message, new()`)

**Rationale**:
- ✅ Properties are settable (IMessage has read-only properties)
- ✅ Can be instantiated with `new()` constraint
- ✅ Simpler mapping from SQS messages
- ✅ Follows pattern established in Messaging.Core

```csharp
// ✅ Can set properties
var message = new OrderMessage
{
    MessageId = sqsMessage.MessageId,
    Body = sqsMessage.Body,
    Properties = attributes,
    EnqueuedTimeUtc = timestamp,
    DeliveryCount = count
};
```

### 3. Rich Metadata Mapping

**Decision**: Map all SQS attributes to `Message.Properties` dictionary

**Rationale**:
- ✅ Access to all SQS metadata (delivery count, timestamps, etc.)
- ✅ Custom message attributes preserved
- ✅ Platform-specific data accessible when needed
- ✅ Supports correlation IDs, tracing, etc.

**Mapped Properties**:
| Property | Source | Use Case |
|----------|---------|----------|
| `EventSource` | SQS EventSource | Identify message origin |
| `EventSourceARN` | SQS ARN | Queue identification |
| `DeliveryCount` | ApproximateReceiveCount | Retry logic |
| `EnqueuedTimeUtc` | SentTimestamp | Age-based processing |
| `CorrelationId` | MessageAttributes | Distributed tracing |
| `SQS_*` | All SQS attributes | Advanced scenarios |

### 4. Batch Failure Handling

**Decision**: Return `SQSBatchResponse` with failed message IDs

**Rationale**:
- ✅ Partial batch failures supported
- ✅ Successful messages not reprocessed
- ✅ Failed messages automatically retried by SQS
- ✅ Reduces processing costs

```csharp
// Failed messages returned for retry
return new SQSBatchResponse(batchItemFailures);
```

### 5. Cancellation Token Support

**Decision**: Use Lambda `RemainingTime` to create cancellation token

**Rationale**:
- ✅ Respects Lambda timeout
- ✅ Graceful shutdown for long-running operations
- ✅ Prevents partial processing
- ✅ Better resource utilization

```csharp
await _handler.HandleAsync(message, context.RemainingTime.ToCancellationToken());
```

## 📊 Comparison with Existing Handlers

| Feature | SqsMessageHandlerBase ⭐ | LambdaMessageHandlerBase | LambdaMessageHandlerBase2 |
|---------|------------------------|-------------------------|--------------------------|
| **Project** | Messaging.Aws | Messaging.Aws | Messaging (core) |
| **Interface** | `IMessageHandler<T>` | `IMessageHandler<T,TContext>` | `ILambdaMessageProcessor<T>` |
| **Pattern** | Publisher-Subscriber | Pub-Sub + Context | Simple Processor |
| **Message Type** | `Message` (settable) | `Message` | Custom `Message` class |
| **Cancellation** | ✅ Yes | ✅ Yes | ❌ No |
| **Batch Failures** | ✅ Yes | ✅ Yes | ✅ Yes |
| **DI Support** | ✅ Full | ✅ Full | ✅ Full |
| **Platform-Agnostic** | ✅ Yes | ✅ Yes | ⚠️ AWS-specific |
| **Metadata** | All via Properties | Context + Properties | Basic attributes |
| **Testing** | Easy (no AWS deps) | Moderate | Moderate |

## 🚀 Usage Example

### 1. Define Message
```csharp
using AppFactory.Framework.Messaging.Abstractions;

public class OrderCreatedMessage : Message
{
    // Inherits: MessageId, Body, Properties, EnqueuedTimeUtc, DeliveryCount
}
```

### 2. Implement Handler
```csharp
public class OrderCreatedHandler : IMessageHandler<OrderCreatedMessage>
{
    private readonly IOrderService _orderService;
    private readonly ILogger _logger;

    public OrderCreatedHandler(IOrderService orderService, ILogger logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedMessage message, CancellationToken ct)
    {
        var orderData = JsonSerializer.Deserialize<OrderData>(message.Body);
        await _orderService.CreateOrderAsync(orderData, ct);
    }
}
```

### 3. Register in Startup
```csharp
public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IMessageHandler<OrderCreatedMessage>, OrderCreatedHandler>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddSerilogLogging();
    }
}
```

### 4. Create Lambda Function
```csharp
public class OrderFunction : SqsMessageHandlerBase<OrderCreatedMessage>
{
    public OrderFunction() : base(new Startup()) { }
    protected override IStartup GetStartup() => new Startup();
    
    public Task<SQSBatchResponse> FunctionHandler(SQSEvent e, ILambdaContext c)
        => Handle(e, c);
}
```

## ✅ Benefits

### 1. Clean Architecture
- **Separation of Concerns**: Business logic (handler) separate from infrastructure (AWS)
- **Testability**: Handlers have no AWS dependencies
- **Maintainability**: Clear boundaries between layers

### 2. Platform Agnostic
- Same `IMessageHandler<T>` works with:
  - AWS SQS (this implementation)
  - Azure Service Bus (future)
  - RabbitMQ (future)
  - Any message broker

### 3. Developer Experience
- **Simple API**: Just implement `HandleAsync(message, ct)`
- **Rich Metadata**: All SQS data available via `Properties`
- **Built-in Logging**: Performance tracking included
- **Error Handling**: Automatic retry logic

### 4. Production Ready
- **Batch Processing**: Handle multiple messages efficiently
- **Partial Failures**: Failed messages don't block successful ones
- **DLQ Support**: Poison messages automatically moved
- **Idempotency**: Message ID available for deduplication

## 🧪 Testing

### Unit Testing (No AWS Dependencies)
```csharp
[Fact]
public async Task HandleAsync_ValidMessage_ProcessesOrder()
{
    // Arrange
    var message = new OrderMessage 
    { 
        MessageId = "test-123",
        Body = JsonSerializer.Serialize(new OrderData { OrderId = "order-123" })
    };
    
    var mockService = new Mock<IOrderService>();
    var handler = new OrderHandler(mockService.Object, Mock.Of<ILogger>());
    
    // Act
    await handler.HandleAsync(message, CancellationToken.None);
    
    // Assert
    mockService.Verify(x => x.CreateOrderAsync(It.IsAny<OrderData>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

## 📈 Migration Path

### From LambdaMessageHandlerBase2

```diff
// Handler
- using AppFactory.Framework.Messaging.LambdaHandlers;
+ using AppFactory.Framework.Messaging.Abstractions;

- public class MyHandler : ILambdaMessageProcessor<MyMessage>
+ public class MyHandler : IMessageHandler<MyMessage>
{
-   public async Task Process(MyMessage message)
+   public async Task HandleAsync(MyMessage message, CancellationToken ct)
    {
        // Business logic (mostly unchanged)
    }
}

// Lambda Function
- using AppFactory.Framework.Messaging.LambdaHandlers;
+ using AppFactory.Framework.Messaging.Aws.Handlers;

- public class Function : LambdaMessageHandlerBase2<MyMessage>
+ public class Function : SqsMessageHandlerBase<MyMessage>
{
    // Constructor and GetStartup() unchanged
}

// Startup
- services.AddScoped<ILambdaMessageProcessor<MyMessage>, MyHandler>();
+ services.AddScoped<IMessageHandler<MyMessage>, MyHandler>();
```

## 🎯 Best Practices

1. ✅ **Implement Idempotency** using `message.MessageId`
2. ✅ **Handle Transient vs Permanent Errors** differently
3. ✅ **Use Cancellation Tokens** to respect Lambda timeout
4. ✅ **Configure Appropriate Visibility Timeout** (>= Lambda timeout)
5. ✅ **Set Up Dead Letter Queue** for poison messages
6. ✅ **Monitor CloudWatch Metrics** (age, message count)
7. ✅ **Use Batch Processing** for better throughput
8. ✅ **Implement Retry with Exponential Backoff** for transient failures

## 📚 Documentation

- **[README.md](./README.md)** - Complete guide with all features
- **[EXAMPLES.md](./EXAMPLES.md)** - 7 real-world examples with code
- **[Messaging.Core](../../AppFactory.Framework.Messaging.Core/README.md)** - Platform-agnostic abstractions

## ✅ Build Status

**All projects build successfully with 0 errors!**

```
✅ AppFactory.Framework.Messaging.Core (abstractions)
✅ AppFactory.Framework.Messaging.Aws (SQS implementation)
✅ AppFactory.Framework.Messaging.Azure (Service Bus implementation)
✅ AppFactory.Framework.Messaging (legacy - unchanged)
```

## 🎉 Summary

You now have:

1. **Platform-Agnostic Handler Interface** (`IMessageHandler<TMessage>`)
2. **AWS SQS Implementation** (`SqsMessageHandlerBase<TMessage>`)
3. **Publisher-Subscriber Pattern** for reactive microservices
4. **Rich Metadata Support** (all SQS attributes mapped)
5. **Production-Ready Features** (batch processing, DLQ, retry)
6. **Comprehensive Documentation** (README + 7 examples)
7. **Clean Architecture** (business logic separate from infrastructure)
8. **Easy Testing** (no AWS dependencies in handlers)

**Write platform-agnostic handlers, deploy to AWS SQS!** 🚀

## 🔄 Related Implementations

This same pattern will be used for:
- ✅ **Azure Service Bus** - `ServiceBusFunctionHandlerBase<TMessage>`
- ✅ **Azure Storage Queue** - `QueueStorageFunctionHandlerBase<TMessage>`
- 🔜 **RabbitMQ** - `RabbitMqMessageHandlerBase<TMessage>` (future)
- 🔜 **Google Cloud Pub/Sub** - `PubSubMessageHandlerBase<TMessage>` (future)

All using the same `IMessageHandler<TMessage>` interface! 🎯
