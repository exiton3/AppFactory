# AppFactory.Framework.Messaging.Azure

**Azure Service Bus and Queue Storage messaging implementation for building reactive microservices on Azure Functions.**

[![NuGet](https://img.shields.io/nuget/v/AppFactory.Framework.Messaging.Azure.svg)](https://www.nuget.org/packages/AppFactory.Framework.Messaging.Azure/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## 📋 Overview

`AppFactory.Framework.Messaging.Azure` provides **Azure-specific** implementations of the platform-agnostic messaging abstractions from `AppFactory.Framework.Messaging.Core`. Build reactive, event-driven microservices on Azure Functions with Service Bus or Queue Storage.

### Key Features

✅ **Azure Service Bus Integration** - Native Service Bus message publishing and consumption  
✅ **Azure Queue Storage Support** - Simple queue-based messaging  
✅ **Azure Functions Handler Base Classes** - Simplified Azure Functions development  
✅ **Automatic Deserialization** - Type-safe message handling  
✅ **Dead Letter Queue Support** - Automatic DLQ integration  
✅ **Batch Publishing** - Efficient batch operations (up to 100 messages for Service Bus)  
✅ **Correlation Tracking** - Built-in distributed tracing support  
✅ **Context-Based Handling** - Complete/Abandon/DeadLetter operations  

---

## 🚀 Installation

```bash
# For Azure Service Bus
dotnet add package AppFactory.Framework.Messaging.Azure --version 10.5.0

# For Queue Storage (same package)
dotnet add package AppFactory.Framework.Messaging.Azure --version 10.5.0
```

**Dependencies:**
```bash
dotnet add package AppFactory.Framework.Messaging.Core --version 10.5.0
```

---

## 💡 Quick Start

### Option 1: Azure Service Bus

#### 1. Configure Services

```csharp
using AppFactory.Framework.Messaging.Azure.Extensions;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Azure Service Bus messaging
        services.AddAzureServiceBus(options =>
        {
            options.ConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnection");
            options.QueueName = "order-queue";
            options.MaxRetries = 3;
            options.MaxBatchSize = 100;
        });

        // Or use configuration
        services.AddAzureServiceBus(Configuration.GetSection("ServiceBus"));
    }
}
```

**local.settings.json** (for local development):
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBusConnection": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key"
  },
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key",
    "QueueName": "order-queue",
    "MaxRetries": 3,
    "MaxBatchSize": 100,
    "EnableSessions": false
  }
}
```

#### 2. Publish Messages

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
        // Create order
        var order = await _orderRepository.AddAsync(new Order { ... });

        // Publish message to Service Bus
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

#### 3. Handle Messages (Simple Azure Function Handler)

```csharp
using AppFactory.Framework.Messaging.Azure.Handlers;

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
        CancellationToken cancellationToken)
    {
        Logger.LogInformation($"Sending confirmation email for order {message.OrderId}");
        await _emailService.SendOrderConfirmationAsync(message.OrderId);
    }
}
```

#### 4. Handle Messages (Context-Based Handler)

```csharp
public class ProcessPaymentFunction : ServiceBusMessageHandlerWithContextBase<OrderCreatedMessage>
{
    private readonly IPaymentService _paymentService;

    public ProcessPaymentFunction(IPaymentService paymentService, ILogger logger) 
        : base(logger)
    {
        _paymentService = paymentService;
    }

    protected override async Task HandleMessageAsync(
        OrderCreatedMessage message,
        IMessageContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            await _paymentService.ProcessPaymentAsync(message.OrderId, message.TotalAmount);
            
            // Explicitly complete (delete from Service Bus)
            await context.CompleteAsync(cancellationToken);
        }
        catch (PaymentDeclinedException ex)
        {
            Logger.LogWarning($"Payment declined for order {message.OrderId}");
            
            // Move to dead letter queue
            await context.DeadLetterAsync("Payment declined", cancellationToken);
        }
        catch (TransientException ex)
        {
            Logger.LogWarning($"Transient error for order {message.OrderId}");
            
            // Abandon - Service Bus will retry
            await context.AbandonAsync(cancellationToken);
        }
    }
}
```

---

### Option 2: Azure Queue Storage

#### 1. Configure Services

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Azure Queue Storage messaging
        services.AddAzureQueueStorage(options =>
        {
            options.ConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            options.QueueName = "order-queue";
            options.VisibilityTimeout = TimeSpan.FromSeconds(30);
            options.TimeToLive = TimeSpan.FromDays(7);
        });
    }
}
```

**local.settings.json**:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  },
  "QueueStorage": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "QueueName": "order-queue",
    "VisibilityTimeout": "00:00:30",
    "TimeToLive": "7.00:00:00"
  }
}
```

#### 2. Publish Messages

```csharp
public class OrderService
{
    private readonly IMessagePublisher _publisher;

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        var message = new OrderCreatedMessage { ... };
        message.AddCorrelationId(request.CorrelationId);
        
        // Publishes to Queue Storage
        await _publisher.PublishAsync(message);
    }
}
```

#### 3. Handle Messages (Queue Storage Function)

```csharp
using AppFactory.Framework.Messaging.Azure.Handlers;

public class ProcessOrderFunction : QueueStorageMessageHandlerBase<OrderCreatedMessage>
{
    private readonly IOrderProcessor _processor;

    public ProcessOrderFunction(IOrderProcessor processor, ILogger logger) 
        : base(logger)
    {
        _processor = processor;
    }

    protected override async Task HandleMessageAsync(
        OrderCreatedMessage message, 
        CancellationToken cancellationToken)
    {
        Logger.LogInformation($"Processing order: {message.OrderId}");
        await _processor.ProcessAsync(message);
    }
}
```

---

## 📦 Azure Functions Setup

### function.json (Not needed for isolated worker)

With .NET isolated worker model, you don't need `function.json`. The `[Function]` attribute handles everything.

### Program.cs

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AppFactory.Framework.Messaging.Azure.Extensions;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Add messaging
        services.AddAzureServiceBus(options =>
        {
            options.ConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnection");
            options.QueueName = "order-queue";
        });

        // Or for Queue Storage
        // services.AddAzureQueueStorage(options => { ... });

        // Add your services
        services.AddScoped<IOrderProcessor, OrderProcessor>();
        services.AddScoped<IEmailService, EmailService>();
    })
    .Build();

await host.RunAsync();
```

### Azure Functions Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AzureFunctionsVersion>v4</AzureFunctionsVersion>
    <OutputType>Exe</OutputType>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Azure.Functions.Worker" Version="2.0.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="2.0.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.ServiceBus" Version="5.16.0" />
    <!-- or -->
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Storage.Queues" Version="5.3.0" />
    
    <PackageReference Include="AppFactory.Framework.Messaging.Azure" Version="10.5.0" />
  </ItemGroup>
</Project>
```

---

## 🔄 Batch Publishing

### Service Bus (Efficient Native Batching)

```csharp
public class BulkOrderProcessor
{
    private readonly IMessagePublisher _publisher;

    public async Task ProcessBulkOrdersAsync(List<Order> orders)
    {
        var messages = orders.Select(o => new OrderCreatedMessage
        {
            OrderId = o.Id,
            TotalAmount = o.TotalAmount
        }).ToList();

        // Service Bus supports up to 100 messages per batch
        var result = await _publisher.PublishBatchAsync(messages);

        if (!result.IsSuccess)
        {
            foreach (var failure in result.Results.Where(r => !r.IsSuccess))
            {
                _logger.LogError($"Failed: {failure.ErrorMessage}");
            }
        }

        _logger.LogInformation($"Published {result.SuccessCount}/{messages.Count} messages");
    }
}
```

### Queue Storage (Parallel Publishing)

```csharp
// Queue Storage doesn't support native batching
// Our implementation sends messages in parallel for better performance
var result = await _publisher.PublishBatchAsync(messages);
// Messages are sent concurrently, up to MaxBatchSize at a time
```

---

## 📊 Service Bus vs Queue Storage Comparison

| Feature | Azure Service Bus | Azure Queue Storage |
|---------|------------------|---------------------|
| **Max Message Size** | 256 KB (std), 100 MB (premium) | 64 KB |
| **Max Batch Size** | 100 messages (native) | No native batching (parallel send) |
| **Dead Letter Queue** | ✅ Built-in | ✅ Poison queue |
| **Message Retention** | 14 days (std), 90 days (premium) | 7 days (max) |
| **Ordering** | ✅ Sessions for FIFO | ❌ No |
| **Transactions** | ✅ Yes | ❌ No |
| **Duplicate Detection** | ✅ Yes | ❌ No |
| **Correlation Tracking** | ✅ Native correlation ID | ✅ Envelope pattern |
| **Pricing** | ~$0.05/million ops | ~$0.01/10k ops |
| **Use Case** | Enterprise messaging, workflows | Simple task queues |
| **AppFactory Support** | ✅ v10.5.0 | ✅ v10.5.0 |

**When to use Service Bus:**
- Need guaranteed ordering (sessions)
- Require transactions
- Need duplicate detection
- Enterprise-grade messaging
- Complex routing scenarios

**When to use Queue Storage:**
- Simple task queues
- Cost-sensitive applications
- Small messages (<64 KB)
- Basic async processing

---

## ⚙️ Configuration Options

### Azure Service Bus Options

```csharp
public class AzureServiceBusOptions
{
    public string ConnectionString { get; set; }      // Required
    public string QueueName { get; set; }             // Required
    public int MaxRetries { get; set; } = 3;
    public bool EnableDetailedLogging { get; set; } = false;
    public int MaxBatchSize { get; set; } = 100;      // 1-100
    public TimeSpan? TimeToLive { get; set; }
    public bool EnableSessions { get; set; } = false; // For FIFO
}
```

### Azure Queue Storage Options

```csharp
public class AzureQueueStorageOptions
{
    public string ConnectionString { get; set; }         // Required
    public string QueueName { get; set; }                // Required
    public int MaxRetries { get; set; } = 3;
    public bool EnableDetailedLogging { get; set; } = false;
    public TimeSpan VisibilityTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan? TimeToLive { get; set; }
    public int MaxBatchSize { get; set; } = 10;          // For parallel sends
}
```

---

## 🔒 Error Handling & Dead Letter Queues

### Service Bus Dead Letter Queue

```bicep
resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2021-11-01' = {
  name: 'order-queue'
  properties: {
    maxDeliveryCount: 3  // Move to DLQ after 3 failures
    deadLetteringOnMessageExpiration: true
  }
}
```

```csharp
protected override async Task HandleMessageAsync(
    OrderCreatedMessage message,
    IMessageContext context,
    CancellationToken ct)
{
    if (message.TotalAmount > 10000)
    {
        // Business rule violation - don't retry
        await context.DeadLetterAsync("Amount exceeds limit", ct);
        return;
    }

    try
    {
        await ProcessOrderAsync(message);
        await context.CompleteAsync(ct);
    }
    catch (TransientException)
    {
        // Transient error - retry
        await context.AbandonAsync(ct);
    }
}
```

### Queue Storage Poison Queue

Azure Functions automatically moves messages to `{queue-name}-poison` after 5 dequeue attempts.

```csharp
// Automatic poison queue handling - no code needed!
// Failed messages automatically move to: order-queue-poison
```

---

## 🧪 Testing

### Local Development with Azurite

```bash
# Install Azurite
npm install -g azurite

# Start Azurite
azurite --silent --location c:\azurite --debug c:\azurite\debug.log

# Or use Docker
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

**local.settings.json**:
```json
{
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true"
  }
}
```

### Mock Service Bus Publisher

```csharp
[Fact]
public async Task CreateOrder_ShouldPublishMessageToServiceBus()
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

### Integration Testing

```csharp
[Fact]
public async Task PublishAsync_ShouldSendMessageToAzurite()
{
    var services = new ServiceCollection();
    services.AddAzureQueueStorage(options =>
    {
        options.ConnectionString = "UseDevelopmentStorage=true";
        options.QueueName = "test-queue";
    });

    var publisher = services.BuildServiceProvider()
        .GetRequiredService<IMessagePublisher>();

    var result = await publisher.PublishAsync(new TestMessage { Data = "test" });

    result.IsSuccess.ShouldBe(true);
}
```

---

## 🌐 Multi-Cloud Support

Use the same business logic across AWS and Azure:

```csharp
// This interface works on AWS SQS, Azure Service Bus, Azure Queue Storage
public class OrderService
{
    private readonly IMessagePublisher _publisher; // Platform-agnostic!

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        var message = new OrderCreatedMessage { ... };
        await _publisher.PublishAsync(message); // Works everywhere!
    }
}
```

**Switch cloud providers by changing registration:**

```csharp
// AWS
services.AddAwsMessaging(options => { ... });

// Azure Service Bus
services.AddAzureServiceBus(options => { ... });

// Azure Queue Storage
services.AddAzureQueueStorage(options => { ... });
```

---

## 📚 Related Packages

| Package | Purpose |
|---------|---------|
| **AppFactory.Framework.Messaging.Core** | Platform-agnostic abstractions |
| **AppFactory.Framework.Messaging.Aws** | AWS SQS + Lambda handlers |
| **AppFactory.Framework.Messaging.Azure** | Azure Service Bus + Queue Storage + Functions (this package) |
| **AppFactory.Framework.EventBus.Azure** | Azure Event Grid (pub/sub events) |

---

## 🔗 Resources

- [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Azure Queue Storage Documentation](https://docs.microsoft.com/azure/storage/queues/)
- [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)
- [Messaging.Core Package](../AppFactory.Framework.Messaging.Core/README.md)
- [GitHub Repository](https://github.com/exiton3/AppFactory)

---

## 💡 Best Practices

### 1. Use Service Bus for Enterprise Scenarios

```csharp
// Complex workflows, ordering requirements, transactions
services.AddAzureServiceBus(options => 
{
    options.EnableSessions = true;  // FIFO ordering
    options.TimeToLive = TimeSpan.FromHours(24);
});
```

### 2. Use Queue Storage for Simple Tasks

```csharp
// Background jobs, simple task queues, cost-sensitive
services.AddAzureQueueStorage(options => 
{
    options.VisibilityTimeout = TimeSpan.FromMinutes(5);
});
```

### 3. Implement Idempotency

```csharp
protected override async Task HandleMessageAsync(OrderCreatedMessage message, ...)
{
    // Check if already processed
    if (await _orderRepository.ExistsAsync(message.OrderId))
    {
        Logger.LogWarning($"Order {message.OrderId} already processed");
        await context.CompleteAsync();
        return;
    }

    await ProcessOrderAsync(message);
}
```

### 4. Use Correlation IDs for Tracing

```csharp
var message = new OrderCreatedMessage { ... };
message.AddCorrelationId(HttpContext.TraceIdentifier);
message.AddUserId(User.GetUserId());

await _publisher.PublishAsync(message);
```

### 5. Configure Dead Letter Queues

```csharp
// Service Bus - configure max delivery count
{
  "maxDeliveryCount": 3
}

// Queue Storage - automatic poison queue after 5 attempts
// Monitor: {queue-name}-poison
```

---

**AppFactory.Framework.Messaging.Azure** - Build Reactive Microservices on Azure! 🚀

*Write once, message everywhere - Azure Service Bus, Queue Storage, and beyond!*
