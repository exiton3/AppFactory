# AppFactory v10.5.0 - Release Notes

**Release Date**: 2024  
**Status**: Production Ready  
**Focus**: Multi-Cloud Messaging Foundation

---

## 🎉 What's New

**Multi-Cloud Reactive Microservices** - Build decoupled, scalable microservices that communicate through queues across AWS SQS, Azure Service Bus, and Azure Queue Storage with the same code!

---

## 🚀 New Features

### 1. **Platform-Agnostic Messaging Abstractions**

New core package provides cloud-independent messaging interfaces:

- **AppFactory.Framework.Messaging.Core** ⭐ NEW - Platform-agnostic abstractions

```csharp
// Write once, works on AWS, Azure, and future platforms
public class OrderService
{
    private readonly IMessagePublisher _publisher;

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        var message = new OrderCreatedMessage { OrderId = "123", TotalAmount = 99.99m };
        message.AddCorrelationId(request.CorrelationId);
        
        await _publisher.PublishAsync(message); // Works everywhere!
    }
}
```

### 2. **AWS SQS Integration**

Production-ready AWS SQS messaging with Lambda support:

- **AppFactory.Framework.Messaging.Aws** ⭐ NEW - AWS SQS implementation

```csharp
// AWS Lambda Handler
public class ProcessOrderFunction : LambdaMessageHandlerBase<OrderCreatedMessage>
{
    protected override async Task HandleMessageAsync(OrderCreatedMessage message, CancellationToken ct)
    {
        await _orderProcessor.ProcessAsync(message);
    }
}
```

**Key Features:**
- SQS publisher with batch support (up to 10 messages)
- Lambda handler base classes (simple + context-based)
- Dead letter queue support
- Automatic message deserialization
- Correlation tracking via SQS message attributes

### 3. **Azure Messaging Integration**

Dual Azure messaging support with Azure Functions:

- **AppFactory.Framework.Messaging.Azure** ⭐ NEW - Service Bus + Queue Storage

```csharp
// Azure Functions Handler (Service Bus)
public class ProcessOrderFunction : ServiceBusMessageHandlerBase<OrderCreatedMessage>
{
    protected override async Task HandleMessageAsync(OrderCreatedMessage message, CancellationToken ct)
    {
        await _orderProcessor.ProcessAsync(message);
    }
}
```

**Key Features:**
- Service Bus publisher with batch support (up to 100 messages)
- Queue Storage publisher with parallel batch support
- Azure Functions handler base classes
- Dead letter queue / poison queue support
- Context-based message operations (Complete/Abandon/DeadLetter)

### 4. **Correlation Tracking**

Built-in distributed tracing support across all platforms:

```csharp
var message = new OrderCreatedMessage { ... };
message.AddCorrelationId(correlationId);
message.AddCausationId(previousMessageId);
message.AddUserId(currentUserId);

await _publisher.PublishAsync(message);

// In handler - automatically populated
var correlationId = message.Properties["CorrelationId"];
var deliveryCount = message.DeliveryCount;
var enqueuedTime = message.EnqueuedTimeUtc;
```

### 5. **Context-Based Message Handling**

Explicit control over message acknowledgment:

```csharp
protected override async Task HandleMessageAsync(
    OrderCreatedMessage message,
    IMessageContext context,
    CancellationToken ct)
{
    try
    {
        await _paymentService.ProcessAsync(message);
        await context.CompleteAsync(ct);  // Success - delete message
    }
    catch (PaymentDeclinedException)
    {
        await context.DeadLetterAsync("Payment declined", ct);  // No retry
    }
    catch (Exception)
    {
        await context.AbandonAsync(ct);  // Retry later
    }
}
```

### 6. **Batch Publishing**

Efficient batch operations for high-throughput scenarios:

```csharp
var messages = orders.Select(o => new OrderCreatedMessage { ... }).ToList();

// AWS SQS: Up to 10 messages per batch
// Azure Service Bus: Up to 100 messages per batch
// Azure Queue Storage: Parallel publishing
var result = await _publisher.PublishBatchAsync(messages);

Console.WriteLine($"Published {result.SuccessCount}/{messages.Count} messages");
```

---

## 📦 Package Updates

**Total Packages**: 24 (3 new Messaging packages added)

### New Packages
- `AppFactory.Framework.Messaging.Core` v10.5.0 ⭐ NEW
- `AppFactory.Framework.Messaging.Aws` v10.5.0 ⭐ NEW
- `AppFactory.Framework.Messaging.Azure` v10.5.0 ⭐ NEW

### Updated Packages
All existing 21 packages updated to v10.5.0 for version consistency.

---

## 🎯 Key Benefits

- ✅ **Zero Vendor Lock-In** - Write code once, deploy to AWS, Azure, or on-premises
- ✅ **Reactive Architecture** - Asynchronous, event-driven microservices
- ✅ **Scalable** - Independent scaling of message producers and consumers
- ✅ **Multi-Cloud Ready** - Same business logic across all cloud providers
- ✅ **Production Ready** - Error handling, DLQ support, retry logic, correlation tracking
- ✅ **Clean Architecture** - Platform-agnostic core with cloud-specific adapters
- ✅ **Developer-Friendly** - Consistent API across all platforms

---

## 💡 Quick Start

### Installation

```bash
# Platform-agnostic core (required)
dotnet add package AppFactory.Framework.Messaging.Core --version 10.5.0

# For AWS SQS
dotnet add package AppFactory.Framework.Messaging.Aws --version 10.5.0

# For Azure Service Bus or Queue Storage
dotnet add package AppFactory.Framework.Messaging.Azure --version 10.5.0
```

### Basic Usage

**1. Configure Services (AWS)**

```csharp
services.AddAwsMessaging(options =>
{
    options.QueueUrl = Environment.GetEnvironmentVariable("QUEUE_URL");
    options.DeadLetterQueueUrl = Environment.GetEnvironmentVariable("DLQ_URL");
    options.MaxRetries = 3;
});
```

**2. Configure Services (Azure Service Bus)**

```csharp
services.AddAzureServiceBus(options =>
{
    options.ConnectionString = Configuration["ServiceBus:ConnectionString"];
    options.QueueName = "order-queue";
    options.MaxBatchSize = 100;
});
```

**3. Configure Services (Azure Queue Storage)**

```csharp
services.AddAzureQueueStorage(options =>
{
    options.ConnectionString = Configuration["AzureWebJobsStorage"];
    options.QueueName = "order-queue";
    options.VisibilityTimeout = TimeSpan.FromSeconds(30);
});
```

**4. Publish Messages (Platform-Agnostic)**

```csharp
public class OrderService
{
    private readonly IMessagePublisher _publisher;

    public OrderService(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        var order = await _orderRepository.AddAsync(new Order { ... });

        var message = new OrderCreatedMessage
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount
        };

        message.AddCorrelationId(request.CorrelationId);
        await _publisher.PublishAsync(message);
    }
}
```

**5. Handle Messages (AWS Lambda)**

```csharp
public class SendOrderConfirmationFunction : LambdaMessageHandlerBase<OrderCreatedMessage>
{
    private readonly IEmailService _emailService;

    public SendOrderConfirmationFunction(IEmailService emailService, ILogger logger) 
        : base(logger)
    {
        _emailService = emailService;
    }

    protected override async Task HandleMessageAsync(
        OrderCreatedMessage message, 
        CancellationToken ct)
    {
        await _emailService.SendOrderConfirmationAsync(message.OrderId);
    }
}
```

**6. Handle Messages (Azure Functions)**

```csharp
public class SendOrderConfirmationFunction : ServiceBusMessageHandlerBase<OrderCreatedMessage>
{
    private readonly IEmailService _emailService;

    public SendOrderConfirmationFunction(IEmailService emailService, ILogger logger) 
        : base(logger)
    {
        _emailService = emailService;
    }

    protected override async Task HandleMessageAsync(
        OrderCreatedMessage message, 
        CancellationToken ct)
    {
        await _emailService.SendOrderConfirmationAsync(message.OrderId);
    }
}
```

---

## 📊 Platform Comparison

| Feature | AWS SQS | Azure Service Bus | Azure Queue Storage |
|---------|---------|-------------------|---------------------|
| **Max Message Size** | 256 KB | 256 KB (std), 100 MB (premium) | 64 KB |
| **Max Batch Size** | 10 messages | 100 messages | No native batching |
| **Dead Letter Queue** | ✅ Built-in | ✅ Built-in | ✅ Poison queue |
| **Message Retention** | 14 days | 14 days (std), 90 days (premium) | 7 days |
| **Ordering** | FIFO queues | Sessions | No |
| **Transactions** | No | ✅ Yes | No |
| **Correlation Tracking** | ✅ Message attributes | ✅ Native correlation ID | ✅ Envelope pattern |
| **Pricing** | ~$0.40/million | ~$0.05/million ops | ~$0.01/10k ops |
| **AppFactory Support** | ✅ v10.5.0 | ✅ v10.5.0 | ✅ v10.5.0 |

---

## 🏗️ Architecture

### Clean Architecture Pattern

```
┌─────────────────────────────────────────────────┐
│        Business Logic (Application Layer)       │
│                                                  │
│  public class OrderService                      │
│  {                                               │
│      private readonly IMessagePublisher _pub;   │
│                                                  │
│      public async Task CreateOrderAsync(...)    │
│      {                                           │
│          await _publisher.PublishAsync(msg);    │
│      }                                           │
│  }                                               │
└───────────────────┬──────────────────────────────┘
                    │ depends on ↓
┌───────────────────┴──────────────────────────────┐
│      Messaging.Core (Platform-Agnostic)          │
│                                                  │
│  - IMessage                                      │
│  - IMessagePublisher                             │
│  - IMessageHandler<T>                            │
│  - IMessageContext                               │
└───────────────────┬────────────────┬─────────────┘
                    │                │
        ┌───────────▼────┐   ┌──────▼──────────┐
        │                │   │                 │
┌───────┴─────────┐  ┌───┴────────────┐
│ Messaging.Aws   │  │ Messaging.Azure │
│                 │  │                 │
│ - SQS           │  │ - Service Bus  │
│ - Lambda        │  │ - Queue Storage│
│ - DLQ           │  │ - Functions    │
└─────────────────┘  └─────────────────┘
```

**Benefits:**
1. ✅ Business logic is cloud-independent
2. ✅ Easy to switch cloud providers
3. ✅ Test without cloud dependencies
4. ✅ Consistent patterns across platforms

---

## 🔒 Breaking Changes

**None!** ✅

This release is **100% backward compatible** with v10.4.0.

**Note**: The existing `AppFactory.Framework.Messaging` package (AWS SQS-only) is still supported but considered legacy. New projects should use the new multi-cloud packages.

---

## 🔄 Migration from v10.4.0

No code changes required for existing functionality! The new messaging packages are **additive** and don't affect existing packages.

### Optional: Adopt Multi-Cloud Messaging

**For new projects:**

```bash
# Install new packages
dotnet add package AppFactory.Framework.Messaging.Core --version 10.5.0
dotnet add package AppFactory.Framework.Messaging.Aws --version 10.5.0
# or
dotnet add package AppFactory.Framework.Messaging.Azure --version 10.5.0
```

**For existing AWS SQS users (using legacy AppFactory.Framework.Messaging):**

The legacy package still works. To migrate to the new multi-cloud approach:

```csharp
// Old (still works)
services.AddScoped<IMessagePublisher, SqsMessagePublisher>();

// New (recommended - multi-cloud ready)
services.AddAwsMessaging(options => 
{
    options.QueueUrl = Configuration["AWS:SQS:QueueUrl"];
});
```

---

## 📚 Documentation

### New Guides
- [Multi-Cloud Messaging Guide](src/AppFactory.Framework.Messaging.Core/README.md)
- [AWS SQS Package README](src/AppFactory.Framework.Messaging.Aws/README.md)
- [Azure Messaging Package README](src/AppFactory.Framework.Messaging.Azure/README.md)

### Updated Documentation
- Main [README.md](README.md) - Updated with messaging examples
- [CHANGELOG.md](CHANGELOG.md) - Complete version history

---

## 🌐 Platform Support

| Platform | Package | Status |
|----------|---------|--------|
| **AWS SQS** | AppFactory.Framework.Messaging.Aws | ✅ v10.5.0 |
| **Azure Service Bus** | AppFactory.Framework.Messaging.Azure | ✅ v10.5.0 |
| **Azure Queue Storage** | AppFactory.Framework.Messaging.Azure | ✅ v10.5.0 |
| **AWS EventBridge** | AppFactory.Framework.EventBus.Aws | ✅ v10.4.0 |
| **Azure Event Grid** | AppFactory.Framework.EventBus.Azure | ✅ v10.4.0 |
| **AWS Lambda** | AppFactory.Framework.Api.Aws | ✅ v10.4.0 |
| **Azure Functions** | AppFactory.Framework.Api.Azure | ✅ v10.4.0 |
| **ASP.NET Core** | AppFactory.Framework.Api.AspNetCore | ✅ v10.4.0 |
| **AWS DynamoDB** | AppFactory.Framework.DataAccess.DynamoDB | ✅ v10.4.0 |
| **Azure Cosmos DB** | AppFactory.Framework.DataAccess.CosmosDB | ✅ v10.4.0 |

---

## 🎯 Use Cases

### 1. Order Processing System

```csharp
// Publisher (API/Command Handler)
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand>
{
    public async Task<CommandResult> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = await _orderRepo.AddAsync(new Order { ... }, ct);
        
        await _publisher.PublishAsync(new OrderCreatedMessage 
        { 
            OrderId = order.Id,
            TotalAmount = order.TotalAmount
        }, ct);
        
        return CommandResult.Success(order.Id);
    }
}

// Consumers (Multiple microservices react)
// ✅ Payment Service → Process payment
// ✅ Email Service → Send confirmation
// ✅ Inventory Service → Reserve items
// ✅ Analytics Service → Track metrics
// ✅ Notification Service → Send push notifications
```

### 2. Background Job Processing

```csharp
// Enqueue background jobs
await _publisher.PublishAsync(new GenerateReportMessage 
{ 
    ReportId = "123",
    UserId = "user-456"
});

// Process in background
public class ReportGeneratorFunction : LambdaMessageHandlerBase<GenerateReportMessage>
{
    protected override async Task HandleMessageAsync(GenerateReportMessage message, ...)
    {
        await _reportService.GenerateAsync(message.ReportId);
    }
}
```

### 3. Event-Driven Workflows

```csharp
// Step 1: User registers
await _publisher.PublishAsync(new UserRegisteredMessage { UserId = "123" });

// Step 2: Send welcome email (handler 1)
await _emailService.SendWelcomeEmailAsync(userId);
await _publisher.PublishAsync(new WelcomeEmailSentMessage { UserId = userId });

// Step 3: Create trial subscription (handler 2)
await _subscriptionService.CreateTrialAsync(userId);
await _publisher.PublishAsync(new TrialCreatedMessage { UserId = userId });

// Each step is independent and can be retried
```

---

## 🔮 What's Next?

### Planned for v10.6.0 (Event Sourcing)
- Event store abstractions
- Aggregate root pattern
- DynamoDB event store implementation
- CosmosDB event store implementation
- Event replay capabilities

### Planned for v10.7.0 (Saga Pattern)
- Saga orchestration engine
- Distributed transaction coordination
- Compensation logic support
- Saga state management

### Planned for v10.8.0 (Transactional Outbox)
- Outbox pattern implementation
- Guaranteed event delivery
- Idempotency support

---

## 📦 Complete Package List (v10.5.0)

| # | Package | Version | Type |
|---|---------|---------|------|
| 1 | AppFactory.Framework.Domain | 10.5.0 | Core |
| 2 | AppFactory.Framework.Application | 10.5.0 | Core |
| 3 | AppFactory.Framework.Shared | 10.5.0 | Core |
| 4 | AppFactory.Framework.DependencyInjection | 10.5.0 | Core |
| 5 | AppFactory.Framework.Logging.Abstractions | 10.5.0 | Logging |
| 6 | AppFactory.Framework.Logging | 10.5.0 | Logging |
| 7 | AppFactory.Framework.Logging.Serilog | 10.5.0 | Logging |
| 8 | AppFactory.Framework.Logging.MicrosoftExtensions | 10.5.0 | Logging |
| 9 | AppFactory.Framework.DataAccess | 10.5.0 | Data |
| 10 | AppFactory.Framework.DataAccess.DynamoDB | 10.5.0 | Data |
| 11 | AppFactory.Framework.DataAccess.CosmosDB | 10.5.0 | Data |
| 12 | AppFactory.Framework.Api | 10.5.0 | API |
| 13 | AppFactory.Framework.Api.Aws | 10.5.0 | API |
| 14 | AppFactory.Framework.Api.Azure | 10.5.0 | API |
| 15 | AppFactory.Framework.Api.AspNetCore | 10.5.0 | API |
| 16 | AppFactory.Framework.Messaging | 10.5.0 | Messaging (legacy) |
| 17 | AppFactory.Framework.EventBus | 10.5.0 | EventBus |
| 18 | AppFactory.Framework.EventBus.Aws | 10.5.0 | EventBus |
| 19 | AppFactory.Framework.EventBus.Azure | 10.5.0 | EventBus |
| 20 | AppFactory.Framework.TestExtensions | 10.5.0 | Testing |
| 21 | AppFactory.Framework.Infrastructure | 10.5.0 | Infrastructure |
| 22 | **AppFactory.Framework.Messaging.Core** | **10.5.0** | **Messaging** ⭐ |
| 23 | **AppFactory.Framework.Messaging.Aws** | **10.5.0** | **Messaging** ⭐ |
| 24 | **AppFactory.Framework.Messaging.Azure** | **10.5.0** | **Messaging** ⭐ |

---

## ✅ Testing

All new packages include comprehensive abstractions and production-ready implementations:
- ✅ 22 unit tests passing (Messaging.Core)
- ✅ Clean Architecture validation
- ✅ Platform compatibility verified
- ✅ Build successful across all 24 packages

---

## 🙏 Acknowledgments

Thank you to the .NET community for continued feedback and support!

---

## 📞 Support

- 🐛 [Report Issues](https://github.com/exiton3/AppFactory/issues)
- 💡 [Request Features](https://github.com/exiton3/AppFactory/issues/new?labels=enhancement)
- 📖 [Documentation](https://github.com/exiton3/AppFactory/wiki)
- ⭐ [Star on GitHub](https://github.com/exiton3/AppFactory)

---

## 🔗 Resources

- **GitHub**: [https://github.com/exiton3/AppFactory](https://github.com/exiton3/AppFactory)
- **NuGet**: [https://www.nuget.org/packages?q=AppFactory.Framework](https://www.nuget.org/packages?q=AppFactory.Framework)
- **License**: MIT
- **Author**: Sergey Kichuk

---

**AppFactory v10.5.0** - Build Multi-Cloud Reactive Microservices! 🚀

*Write once, message everywhere - AWS, Azure, and beyond!*
