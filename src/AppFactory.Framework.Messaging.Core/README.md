# AppFactory.Framework.Messaging.Core

**Platform-agnostic messaging abstractions for building multi-cloud reactive microservices.**

[![NuGet](https://img.shields.io/nuget/v/AppFactory.Framework.Messaging.Core.svg)](https://www.nuget.org/packages/AppFactory.Framework.Messaging.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## 📋 Overview

`AppFactory.Framework.Messaging.Core` provides **platform-agnostic abstractions** for queue-based messaging in distributed systems. Build reactive microservices that work seamlessly across **AWS SQS**, **Azure Service Bus**, **Azure Queue Storage**, and other message queues without vendor lock-in.

### Key Features

✅ **Multi-Cloud Ready** - Write once, deploy anywhere (AWS, Azure, GCP)  
✅ **Correlation Tracking** - Built-in support for distributed tracing  
✅ **Type-Safe** - Strongly-typed messages and handlers  
✅ **Batch Support** - Efficient batch publishing for high throughput  
✅ **Context-Based Handling** - Complete/Abandon/DeadLetter message operations  
✅ **Clean Architecture** - Platform-agnostic core with cloud-specific adapters  

---

## 🚀 Installation

```bash
dotnet add package AppFactory.Framework.Messaging.Core --version 10.5.0
```

**Cloud-Specific Implementations:**
```bash
# For AWS SQS
dotnet add package AppFactory.Framework.Messaging.Aws --version 10.5.0

# For Azure Service Bus
dotnet add package AppFactory.Framework.Messaging.Azure --version 10.5.0
```

---

## 📦 Core Abstractions

### `IMessage`

Platform-agnostic message interface with correlation tracking:

```csharp
public interface IMessage
{
    string MessageId { get; set; }
    Dictionary<string, string> Properties { get; set; }
    DateTime EnqueuedTimeUtc { get; set; }
    int DeliveryCount { get; set; }
}
```

### `IMessagePublisher`

Publisher abstraction for queue-based messaging:

```csharp
public interface IMessagePublisher
{
    Task<PublishResult> PublishAsync<TMessage>(
        TMessage message, 
        CancellationToken cancellationToken = default) 
        where TMessage : class;

    Task<BatchPublishResult> PublishBatchAsync<TMessage>(
        IEnumerable<TMessage> messages, 
        CancellationToken cancellationToken = default) 
        where TMessage : class;
}
```

### `IMessageHandler<TMessage>`

Simple message handler for fire-and-forget scenarios:

```csharp
public interface IMessageHandler<in TMessage> where TMessage : class
{
    Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);
}
```

### `IMessageHandler<TMessage, TContext>`

Context-based handler with Complete/Abandon/DeadLetter support:

```csharp
public interface IMessageHandler<in TMessage, in TContext> 
    where TMessage : class
    where TContext : IMessageContext
{
    Task HandleAsync(
        TMessage message, 
        TContext context, 
        CancellationToken cancellationToken = default);
}
```

### `IMessageContext`

Message processing context for explicit acknowledgment:

```csharp
public interface IMessageContext
{
    Task CompleteAsync(CancellationToken cancellationToken = default);
    Task AbandonAsync(CancellationToken cancellationToken = default);
    Task DeadLetterAsync(string reason, CancellationToken cancellationToken = default);
}
```

---

## 💡 Usage Examples

### 1. Define a Message

```csharp
public class OrderCreatedMessage : Message
{
    public string OrderId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string CustomerId { get; set; } = string.Empty;
}
```

### 2. Publish a Message

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
        // 1. Create order in database
        var order = await _orderRepository.AddAsync(new Order { ... });

        // 2. Publish message
        var message = new OrderCreatedMessage
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            CustomerId = order.CustomerId
        };

        message.AddCorrelationId(request.CorrelationId);
        message.AddUserId(request.UserId);

        await _publisher.PublishAsync(message);
    }
}
```

### 3. Handle a Message (Simple)

```csharp
public class SendOrderConfirmationHandler : IMessageHandler<OrderCreatedMessage>
{
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendOrderConfirmationHandler(IEmailService emailService, ILogger logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Sending order confirmation for {message.OrderId}");
        await _emailService.SendOrderConfirmationAsync(message.OrderId);
    }
}
```

### 4. Handle a Message (Context-Based)

```csharp
public class ProcessPaymentHandler : IMessageHandler<OrderCreatedMessage, IMessageContext>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger _logger;

    public async Task HandleAsync(
        OrderCreatedMessage message, 
        IMessageContext context, 
        CancellationToken cancellationToken)
    {
        try
        {
            await _paymentService.ProcessPaymentAsync(message.OrderId, message.TotalAmount);
            await context.CompleteAsync(cancellationToken);
        }
        catch (PaymentDeclinedException ex)
        {
            _logger.LogWarning($"Payment declined for order {message.OrderId}");
            await context.DeadLetterAsync("Payment declined", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing payment for order {message.OrderId}");
            await context.AbandonAsync(cancellationToken); // Retry later
        }
    }
}
```

### 5. Register Message Handlers

```csharp
// Manual registration
services.AddMessageHandler<SendOrderConfirmationHandler, OrderCreatedMessage>();
services.AddMessageHandler<ProcessPaymentHandler, OrderCreatedMessage>();

// Automatic assembly scanning
services.AddMessageHandlers(typeof(Program).Assembly);
```

### 6. Batch Publishing

```csharp
public class BulkOrderService
{
    private readonly IMessagePublisher _publisher;

    public async Task ProcessBulkOrdersAsync(List<Order> orders)
    {
        var messages = orders.Select(o => new OrderCreatedMessage
        {
            OrderId = o.Id,
            TotalAmount = o.TotalAmount,
            CustomerId = o.CustomerId
        }).ToList();

        var result = await _publisher.PublishBatchAsync(messages);

        if (!result.IsSuccess)
        {
            // Handle failed messages
            foreach (var failure in result.FailedMessages)
            {
                _logger.LogError($"Failed to publish message {failure.MessageId}: {failure.ErrorMessage}");
            }
        }
    }
}
```

---

## 🏗️ Architecture

### Platform-Agnostic Design

```
┌─────────────────────────────────────────┐
│   Business Logic (Your Code)           │
│   - OrderService                        │
│   - SendOrderConfirmationHandler        │
└─────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│   Messaging.Core (Abstractions)         │
│   - IMessage                            │
│   - IMessagePublisher                   │
│   - IMessageHandler<T>                  │
└─────────────────────────────────────────┘
                  │
        ┌─────────┴─────────┐
        ▼                   ▼
┌──────────────────┐  ┌──────────────────┐
│  Messaging.Aws   │  │  Messaging.Azure │
│  - SqsPublisher  │  │  - ServiceBus    │
│  - Lambda        │  │  - Functions     │
└──────────────────┘  └──────────────────┘
```

### Benefits

1. **Vendor Independence** - Switch cloud providers without changing business logic
2. **Testability** - Mock `IMessagePublisher` and `IMessageHandler` in unit tests
3. **Consistency** - Same patterns across all cloud platforms
4. **Flexibility** - Add new cloud providers without modifying core logic

---

## 🔄 Messaging vs Events

| Feature | **Messaging** (Queue) | **Events** (Pub/Sub) |
|---------|----------------------|---------------------|
| **Pattern** | Point-to-point | Publish-Subscribe |
| **Delivery** | Single consumer | Multiple subscribers |
| **Ordering** | FIFO (optional) | No guarantees |
| **Retry** | Built-in | Manual |
| **Use Case** | Commands, Tasks | Notifications, Integration |
| **AWS** | SQS | EventBridge |
| **Azure** | Service Bus, Queue Storage | Event Grid |
| **AppFactory** | `Messaging.Core` | `EventBus` |

**When to use Messaging:**
- Task processing (send emails, generate reports)
- Command handling (create order, update inventory)
- Work queue patterns (background jobs)
- Load leveling and buffering

**When to use Events:**
- Domain events (user created, order shipped)
- Cross-service integration
- Event-driven workflows
- Audit logging

---

## 🧪 Testing

### Mock Publisher in Unit Tests

```csharp
[Fact]
public async Task CreateOrder_ShouldPublishOrderCreatedMessage()
{
    // Arrange
    var mockPublisher = new Mock<IMessagePublisher>();
    var service = new OrderService(mockPublisher.Object);

    // Act
    await service.CreateOrderAsync(new CreateOrderRequest { ... });

    // Assert
    mockPublisher.Verify(p => p.PublishAsync(
        It.Is<OrderCreatedMessage>(m => m.OrderId == "123"),
        It.IsAny<CancellationToken>()), Times.Once);
}
```

### Test Handlers

```csharp
[Fact]
public async Task Handler_ShouldSendEmailForOrderCreatedMessage()
{
    // Arrange
    var mockEmailService = new Mock<IEmailService>();
    var handler = new SendOrderConfirmationHandler(mockEmailService.Object);
    var message = new OrderCreatedMessage { OrderId = "123" };

    // Act
    await handler.HandleAsync(message, CancellationToken.None);

    // Assert
    mockEmailService.Verify(s => s.SendOrderConfirmationAsync("123"), Times.Once);
}
```

---

## 📊 Correlation Tracking

Track requests across distributed services:

```csharp
// Service A: Publish message with correlation ID
var message = new OrderCreatedMessage { ... };
message.AddCorrelationId(Guid.NewGuid().ToString());
message.AddUserId(currentUserId);
await _publisher.PublishAsync(message);

// Service B: Extract correlation ID for logging
public async Task HandleAsync(OrderCreatedMessage message, CancellationToken ct)
{
    var correlationId = message.Properties["CorrelationId"];
    _logger.LogInformation($"[{correlationId}] Processing order {message.OrderId}");
    
    // Pass to downstream services
    var nextMessage = new PaymentProcessedMessage { ... };
    nextMessage.AddCorrelationId(correlationId);
    nextMessage.AddCausationId(message.MessageId); // Track message chain
    await _publisher.PublishAsync(nextMessage);
}
```

---

## 🌐 Multi-Cloud Examples

### AWS SQS (using Messaging.Aws)

```csharp
services.AddAwsMessaging(options =>
{
    options.QueueUrl = Configuration["AWS:SQS:QueueUrl"];
});
```

### Azure Service Bus (using Messaging.Azure)

```csharp
services.AddAzureServiceBus(options =>
{
    options.ConnectionString = Configuration["Azure:ServiceBus:ConnectionString"];
    options.QueueName = "orders";
});
```

### Same Business Logic Works Everywhere

```csharp
// This code works on AWS Lambda, Azure Functions, or ASP.NET Core
public class OrderService
{
    private readonly IMessagePublisher _publisher; // Platform-agnostic

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        var message = new OrderCreatedMessage { ... };
        await _publisher.PublishAsync(message); // Works everywhere!
    }
}
```

---

## 📚 Related Packages

| Package | Purpose |
|---------|---------|
| **AppFactory.Framework.Messaging.Core** | Platform-agnostic abstractions (this package) |
| **AppFactory.Framework.Messaging.Aws** | AWS SQS implementation + Lambda handlers |
| **AppFactory.Framework.Messaging.Azure** | Azure Service Bus + Queue Storage + Functions |
| **AppFactory.Framework.EventBus** | Event-driven pub/sub (EventBridge, Event Grid) |
| **AppFactory.Framework.EventSourcing** | Event sourcing and aggregate roots |
| **AppFactory.Framework.Sagas** | Distributed transaction coordination |

---

## 🔗 Resources

- [Multi-Cloud Messaging Guide](../../docs/MULTI_CLOUD_MESSAGING.md)
- [Event-Driven Architecture Guide](../../EVENT_DRIVEN_ARCHITECTURE_GUIDE.md)
- [Messaging vs Events Comparison](../../docs/MESSAGING_VS_EVENTS.md)
- [GitHub Repository](https://github.com/exiton3/AppFactory)

---

## 📞 Support

- 🐛 [Report Issues](https://github.com/exiton3/AppFactory/issues)
- 💡 [Request Features](https://github.com/exiton3/AppFactory/issues/new?labels=enhancement)
- ⭐ [Star on GitHub](https://github.com/exiton3/AppFactory)

---

**AppFactory.Framework.Messaging.Core** - Build Reactive Microservices, Deploy Anywhere! 🚀

*Write once, message everywhere!*
