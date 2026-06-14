# Unified Multi-Cloud Messaging Architecture

## 🎯 Executive Summary

You now have a **100% cloud-agnostic** messaging architecture. Write your message processor **ONCE**, deploy it to:
- ✅ **AWS Lambda** + SQS
- ✅ **Azure Functions** + Service Bus Queue
- ✅ **Azure Functions** + Service Bus Topic
- ✅ **Azure Functions** + Storage Queue

## 🏗️ Architecture Overview

```
┌──────────────────────────────────────────────────────────────────────────┐
│                     IMessageProcessor<TMessage>                          │
│                  (Cloud-Agnostic Business Logic)                         │
│                                                                          │
│  Your implementation: OrderCreatedProcessor, PaymentProcessor, etc.     │
└────────────────────────────────┬─────────────────────────────────────────┘
                                 │
                                 │ Used by
                 ┌───────────────┼───────────────┐
                 │               │               │
     ┌───────────▼──────┐   ┌────▼─────┐   ┌────▼─────────────┐
     │ AWS Lambda       │   │ Azure    │   │ Azure Functions  │
     │ SQS Handler      │   │ Service  │   │ Storage Queue    │
     │                  │   │ Bus      │   │ Handler          │
     │ Base Class:      │   │ Handler  │   │                  │
     │ Lambda           │   │          │   │ Base Class:      │
     │ MessageHandler   │   │ Base     │   │ QueueStorage     │
     │ Base2<T>         │   │ Class:   │   │ FunctionHandler  │
     │                  │   │ Service  │   │ Base<T>          │
     └──────────────────┘   │ Bus      │   └──────────────────┘
                            │ Function │
                            │ Handler  │
                            │ Base<T>  │
                            └──────────┘
                                 │
                 ┌───────────────┴────────────────┐
                 │                                │
        ┌────────▼──────────┐          ┌─────────▼──────────┐
        │ Service Bus Queue │          │ Service Bus Topic  │
        │ Messages          │          │ + Subscriptions    │
        └───────────────────┘          └────────────────────┘
```

## 📋 Interface Hierarchy

```csharp
// Cloud-agnostic (NEW - use this!)
public interface IMessageProcessor<in TMessage> where TMessage : Message
{
    Task Process(TMessage message);
}

// AWS-specific (backward compatible)
public interface ILambdaMessageProcessor<T> : IMessageProcessor<T> where T : Message
{
    // Inherits: Task Process(T message);
}
```

**Migration Strategy**: Both interfaces are supported. New code should use `IMessageProcessor<TMessage>`.

## 🚀 Complete Example: Order Processing System

### 1. Define Your Message Type

```csharp
using AppFactory.Framework.Messaging.LambdaHandlers;

public class OrderCreatedMessage : Message
{
    // Message class provides:
    // - string Body
    // - string MessageId
    // - string Source
    // - Dictionary<string, string> Attributes
}

public class OrderData
{
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
}
```

### 2. Implement Cloud-Agnostic Processor

```csharp
using AppFactory.Framework.Messaging.MessageProcessors;
using AppFactory.Framework.Logging;
using System.Text.Json;

/// <summary>
/// CLOUD-AGNOSTIC processor - works on AWS Lambda AND Azure Functions
/// </summary>
public class OrderCreatedProcessor : IMessageProcessor<OrderCreatedMessage>
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger _logger;

    public OrderCreatedProcessor(
        IOrderService orderService,
        IPaymentService paymentService,
        ILogger logger)
    {
        _orderService = orderService;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task Process(OrderCreatedMessage message)
    {
        _logger.LogInfo($"Processing order created message: {message.MessageId}");
        
        // Deserialize the message body
        var orderData = JsonSerializer.Deserialize<OrderData>(message.Body);
        
        // Access cloud-agnostic metadata
        var correlationId = message.Attributes.GetValueOrDefault("CorrelationId");
        var deliveryCount = message.Attributes.GetValueOrDefault("DeliveryCount");
        var enqueuedTime = message.Attributes.GetValueOrDefault("EnqueuedTimeUtc");
        
        _logger.LogInfo($"Order {orderData.OrderId}, CorrelationId: {correlationId}, Delivery: {deliveryCount}");
        
        // Execute business logic - 100% cloud-agnostic
        try
        {
            await _orderService.CreateOrderAsync(orderData);
            await _paymentService.InitiatePaymentAsync(orderData.OrderId, orderData.Amount);
            
            _logger.LogInfo($"Order {orderData.OrderId} processed successfully");
        }
        catch (InvalidDataException ex)
        {
            _logger.LogError(ex, $"Invalid order data for {orderData.OrderId} - moving to DLQ");
            throw; // Will move to Dead Letter Queue
        }
        catch (PaymentException ex)
        {
            _logger.LogError(ex, $"Payment failed for order {orderData.OrderId} - will retry");
            throw; // Will retry based on queue configuration
        }
    }
}
```

### 3. Register in Startup (Cloud-Agnostic)

```csharp
using AppFactory.Framework.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register cloud-agnostic processor
        services.AddScoped<IMessageProcessor<OrderCreatedMessage>, OrderCreatedProcessor>();
        
        // Register business services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IOrderRepository, DynamoDbOrderRepository>();
        
        // Add logging (works on both AWS and Azure)
        services.AddSingleton<ILoggerFactory, SerilogLoggerFactory>();
        services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger());
    }
}
```

### 4A. Deploy to AWS Lambda + SQS

```csharp
using AppFactory.Framework.Messaging.LambdaHandlers;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace OrderService.Lambda;

public class OrderCreatedHandler : LambdaMessageHandlerBase2<OrderCreatedMessage>
{
    public OrderCreatedHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    // AWS Lambda entry point
    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent evt, ILambdaContext ctx)
        => await Handle(evt, ctx);
}
```

**AWS Configuration (serverless.yml)**:
```yaml
functions:
  processOrderCreated:
    handler: OrderService.Lambda::OrderService.Lambda.OrderCreatedHandler::FunctionHandler
    events:
      - sqs:
          arn: !GetAtt OrderCreatedQueue.Arn
          batchSize: 10
          functionResponseType: ReportBatchItemFailures

resources:
  Resources:
    OrderCreatedQueue:
      Type: AWS::SQS::Queue
      Properties:
        QueueName: OrderCreatedQueue
        RedrivePolicy:
          deadLetterTargetArn: !GetAtt OrderCreatedDLQ.Arn
          maxReceiveCount: 3
```

### 4B. Deploy to Azure Functions + Service Bus Queue

```csharp
using AppFactory.Framework.Messaging.Azure.FunctionHandlers;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;

namespace OrderService.AzureFunctions;

public class OrderCreatedHandler : ServiceBusFunctionHandlerBase<OrderCreatedMessage>
{
    public OrderCreatedHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    [Function("ProcessOrderCreated")]
    public async Task Run(
        [ServiceBusTrigger("order-created-queue", Connection = "ServiceBusConnection")] 
        ServiceBusReceivedMessage message,
        FunctionContext context)
        => await Handle(message, context);
}
```

**Azure Configuration (local.settings.json)**:
```json
{
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBusConnection": "<your-service-bus-connection-string>"
  }
}
```

### 4C. Deploy to Azure Functions + Service Bus Topic

```csharp
public class OrderCreatedEventHandler : ServiceBusFunctionHandlerBase<OrderCreatedMessage>
{
    public OrderCreatedEventHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    [Function("ProcessOrderCreatedEvent")]
    public async Task Run(
        [ServiceBusTrigger("order-events-topic", "order-processor-subscription", Connection = "ServiceBusConnection")] 
        ServiceBusReceivedMessage message,
        FunctionContext context)
        => await HandleTopicMessage(message, context);
}
```

### 4D. Deploy to Azure Functions + Storage Queue

```csharp
using AppFactory.Framework.Messaging.Azure.FunctionHandlers;
using Azure.Storage.Queues.Models;

public class OrderCreatedQueueHandler : QueueStorageFunctionHandlerBase<OrderCreatedMessage>
{
    public OrderCreatedQueueHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    [Function("ProcessOrderCreatedQueue")]
    public async Task Run(
        [QueueTrigger("order-created-queue", Connection = "AzureWebJobsStorage")] 
        QueueMessage message,
        FunctionContext context)
        => await Handle(message, context);
}
```

## 📊 Feature Comparison

| Feature | AWS Lambda | Service Bus Queue | Service Bus Topic | Storage Queue |
|---------|-----------|------------------|------------------|---------------|
| Batch Processing | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| Dead Letter Queue | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| Retry Logic | ✅ Auto | ✅ Auto | ✅ Auto | ✅ Auto |
| Correlation ID | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| Session Support | ❌ No | ✅ Yes | ✅ Yes | ❌ No |
| Message Ordering | ❌ No | ✅ Yes (FIFO) | ❌ No | ❌ No |
| Pub/Sub Pattern | ❌ No | ❌ No | ✅ Yes | ❌ No |
| Cost | 💰 Low | 💰💰 Medium | 💰💰 Medium | 💰 Low |

## 🔄 Message Metadata Mapping

All platforms provide consistent access to message metadata through `Message.Attributes`:

| Attribute Key | AWS SQS | Service Bus | Storage Queue | Description |
|--------------|---------|-------------|---------------|-------------|
| `MessageId` | ✅ | ✅ | ✅ | Unique message identifier |
| `DeliveryCount` | `ApproximateReceiveCount` | `DeliveryCount` | `DequeueCount` | Number of delivery attempts |
| `EnqueuedTimeUtc` | `SentTimestamp` | `EnqueuedTime` | `InsertedOn` | When message was enqueued |
| `CorrelationId` | Custom attribute | `CorrelationId` | Custom metadata | For message correlation |
| `SessionId` | ❌ | `SessionId` | ❌ | Service Bus sessions |
| `SequenceNumber` | ❌ | `SequenceNumber` | ❌ | Message sequence |
| `Subject` | ❌ | `Subject` | ❌ | Message subject/topic |

## ✅ Benefits of This Architecture

### 1. **True Multi-Cloud**
Deploy the **same code** to AWS and Azure without changes.

### 2. **Easy Testing**
Test processors without cloud dependencies:

```csharp
[Fact]
public async Task Process_ValidOrder_CreatesOrder()
{
    // Arrange
    var message = new OrderCreatedMessage
    {
        Body = JsonSerializer.Serialize(new OrderData { OrderId = "123", Amount = 99.99m }),
        MessageId = "msg-001",
        Source = "test",
        Attributes = new Dictionary<string, string>
        {
            ["CorrelationId"] = "corr-123",
            ["DeliveryCount"] = "1"
        }
    };
    
    var mockOrderService = new Mock<IOrderService>();
    var mockPaymentService = new Mock<IPaymentService>();
    var processor = new OrderCreatedProcessor(
        mockOrderService.Object, 
        mockPaymentService.Object, 
        Mock.Of<ILogger>());
    
    // Act
    await processor.Process(message);
    
    // Assert
    mockOrderService.Verify(x => x.CreateOrderAsync(It.Is<OrderData>(o => o.OrderId == "123")), Times.Once);
    mockPaymentService.Verify(x => x.InitiatePaymentAsync("123", 99.99m), Times.Once);
}
```

### 3. **Backward Compatible**
Existing code using `ILambdaMessageProcessor<T>` continues to work.

### 4. **Consistent Patterns**
All base classes follow the same structure:
- Constructor with `IStartup`
- `GetStartup()` abstract method
- `Handle()` / `HandleTopicMessage()` / `HandleBatch()` methods
- Automatic DI scope creation
- Performance logging
- Error handling

### 5. **Migration Path**
Easy to migrate from AWS-only to multi-cloud:

```diff
- public class OrderProcessor : ILambdaMessageProcessor<OrderMessage>
+ public class OrderProcessor : IMessageProcessor<OrderMessage>
```

```diff
- services.AddScoped<ILambdaMessageProcessor<OrderMessage>, OrderProcessor>();
+ services.AddScoped<IMessageProcessor<OrderMessage>, OrderProcessor>();
```

## 🎯 Best Practices

### 1. **Use IMessageProcessor for New Code**
```csharp
// ✅ GOOD - Cloud-agnostic
public class MyProcessor : IMessageProcessor<MyMessage>

// ⚠️ LEGACY - AWS-specific naming (still works)
public class MyProcessor : ILambdaMessageProcessor<MyMessage>
```

### 2. **Register Both Interfaces for Maximum Compatibility**
```csharp
services.AddScoped<IMessageProcessor<OrderMessage>, OrderProcessor>();
// ILambdaMessageProcessor<T> automatically works because it extends IMessageProcessor<T>
```

### 3. **Handle Retries Gracefully**
```csharp
public async Task Process(OrderMessage message)
{
    var deliveryCount = int.Parse(message.Attributes.GetValueOrDefault("DeliveryCount", "1"));
    
    if (deliveryCount > 3)
    {
        _logger.LogWarning($"Message {message.MessageId} has been retried {deliveryCount} times");
        // Implement exponential backoff or send to DLQ
    }
    
    // ... process message
}
```

### 4. **Use Correlation IDs**
```csharp
var correlationId = message.Attributes.GetValueOrDefault("CorrelationId");
_logger.AddProperty("CorrelationId", correlationId);
// All subsequent logs will include the CorrelationId
```

### 5. **Implement Idempotency**
```csharp
public async Task Process(OrderMessage message)
{
    var orderData = JsonSerializer.Deserialize<OrderData>(message.Body);
    
    // Check if already processed
    if (await _orderRepository.ExistsAsync(orderData.OrderId))
    {
        _logger.LogInfo($"Order {orderData.OrderId} already processed - skipping");
        return;
    }
    
    // Process order
    await _orderService.CreateOrderAsync(orderData);
}
```

## 📁 Project Structure

```
YourSolution/
├── OrderService.Core/                    # Shared business logic
│   ├── Messages/
│   │   └── OrderCreatedMessage.cs
│   ├── Processors/
│   │   └── OrderCreatedProcessor.cs     # ⭐ Cloud-agnostic processor
│   ├── Services/
│   │   ├── IOrderService.cs
│   │   └── OrderService.cs
│   └── Startup.cs                        # ⭐ Shared DI configuration
│
├── OrderService.Lambda/                  # AWS Lambda deployment
│   ├── Functions/
│   │   └── OrderCreatedHandler.cs       # Extends LambdaMessageHandlerBase2<T>
│   └── serverless.yml
│
└── OrderService.AzureFunctions/          # Azure Functions deployment
    ├── Functions/
    │   ├── OrderCreatedQueueHandler.cs  # Extends ServiceBusFunctionHandlerBase<T>
    │   └── OrderCreatedTopicHandler.cs  # Extends ServiceBusFunctionHandlerBase<T>
    └── local.settings.json
```

## 🚀 Deployment Commands

### AWS Lambda
```bash
# Build
dotnet build OrderService.Lambda

# Deploy with Serverless Framework
cd OrderService.Lambda
serverless deploy --stage dev

# Or with AWS SAM
sam build
sam deploy --guided
```

### Azure Functions
```bash
# Build
dotnet build OrderService.AzureFunctions

# Run locally
cd OrderService.AzureFunctions
func start

# Deploy to Azure
func azure functionapp publish <your-function-app-name>
```

## 📊 Performance Considerations

### Batch Processing

All base classes support batch processing:

**AWS Lambda:**
```csharp
// Configure in serverless.yml
events:
  - sqs:
      batchSize: 10  # Process up to 10 messages at once
      maxBatchingWindowInSeconds: 5
```

**Azure Service Bus:**
```csharp
[ServiceBusTrigger("queue-name", Connection = "ServiceBusConnection", 
    IsBatched = true)]  // Enable batch processing
ServiceBusReceivedMessage[] messages
```

### Cold Start Optimization

```csharp
public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Use Singleton for expensive-to-create services
        services.AddSingleton<ILoggerFactory, SerilogLoggerFactory>();
        services.AddSingleton<ICosmosDbClient, CosmosDbClient>();
        
        // Use Scoped for per-message instances
        services.AddScoped<IMessageProcessor<OrderMessage>, OrderProcessor>();
        services.AddScoped<IOrderService, OrderService>();
    }
}
```

## 🔧 Troubleshooting

### Processor Not Found
```
InvalidOperationException: No service for type IMessageProcessor<OrderMessage>
```
**Solution**: Register processor in `Startup.ConfigureServices()`:
```csharp
services.AddScoped<IMessageProcessor<OrderMessage>, OrderProcessor>();
```

### Namespace Collision (Azure)
```
CS0104: 'ServiceBus' is an ambiguous reference
```
**Solution**: Use namespace alias (already done in base classes):
```csharp
using AzureServiceBus = Azure.Messaging.ServiceBus;
```

### Message Properties Read-Only
```
Cannot assign to property 'Body' because it is read-only
```
**Solution**: Use concrete `Message` class, not `IMessage` interface:
```csharp
var message = new OrderMessage  // Not: IMessage
{
    Body = "...",
    MessageId = "..."
};
```

## 📚 Related Documentation

- [AWS Lambda Handlers README](../AppFactory.Framework.Messaging/LambdaHandlers/README.md)
- [Azure Function Handlers README](../AppFactory.Framework.Messaging.Azure/FunctionHandlers/README.md)
- [Message Processors README](../AppFactory.Framework.Messaging/MessageProcessors/README.md)
- [Message Publishing Guide](../AppFactory.Framework.Messaging.Core/README.md)

## 🎉 Summary

You now have a **production-ready, cloud-agnostic messaging architecture**:

1. ✅ Write processor **once** using `IMessageProcessor<TMessage>`
2. ✅ Deploy to **AWS Lambda**, **Azure Service Bus**, or **Azure Storage Queue**
3. ✅ Same DI configuration across all platforms
4. ✅ Backward compatible with existing `ILambdaMessageProcessor<T>` code
5. ✅ Comprehensive error handling and retry logic
6. ✅ Performance logging built-in
7. ✅ Easy to test without cloud dependencies

**Next Steps**:
1. Implement your message types
2. Create cloud-agnostic processors
3. Deploy to your preferred cloud (or both!)
4. Monitor and scale as needed
