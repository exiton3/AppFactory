# AWS SQS Message Handler

## Overview

AWS Lambda handler for processing messages from Amazon SQS using the **Publisher-Subscriber pattern** with platform-agnostic message handler interface.

## Architecture

```
┌──────────────────────────────────────────────────────┐
│         IMessageHandler<TMessage>                    │
│         (Platform-Agnostic Business Logic)           │
│         - Defined in Messaging.Core                  │
└─────────────────────┬────────────────────────────────┘
                      │
                      │ Used by
          ┌───────────▼──────────────┐
          │  SqsMessageHandlerBase   │
          │  (AWS SQS Integration)   │
          │  - Maps SQS → IMessage   │
          │  - Batch failure handling│
          │  - DI scope per message  │
          └──────────────────────────┘
                      │
                      │ Processes
          ┌───────────▼──────────────┐
          │    Amazon SQS Queue       │
          │    - FIFO or Standard     │
          │    - Dead Letter Queue    │
          └───────────────────────────┘
```

## Key Features

✅ **Platform-Agnostic Handlers** - Business logic uses `IMessageHandler<T>` from `Messaging.Core`  
✅ **Automatic Message Mapping** - SQS messages → `IMessage` with metadata  
✅ **Batch Failure Handling** - Returns failed message IDs for SQS retry  
✅ **Dependency Injection** - Scoped service per message  
✅ **Performance Logging** - Built-in execution time tracking  
✅ **Cancellation Support** - Uses Lambda remaining time  
✅ **Rich Metadata** - All SQS attributes accessible via `Properties`

## Usage Example

### 1. Define Your Message

```csharp
using AppFactory.Framework.Messaging.Abstractions;

public class OrderCreatedMessage : Message
{
    // Inherits from Message class in Messaging.Core:
    // - string MessageId
    // - string Body
    // - IDictionary<string, string> Properties
    // - DateTime EnqueuedTimeUtc
    // - int DeliveryCount
}

// Your domain data (serialized in Body)
public class OrderData
{
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CustomerId { get; set; }
}
```

### 2. Implement Platform-Agnostic Handler

```csharp
using AppFactory.Framework.Messaging.Abstractions;
using AppFactory.Framework.Logging;
using System.Text.Json;

/// <summary>
/// Platform-agnostic message handler - can work with ANY message broker
/// </summary>
public class OrderCreatedHandler : IMessageHandler<OrderCreatedMessage>
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger _logger;

    public OrderCreatedHandler(
        IOrderService orderService,
        IPaymentService paymentService,
        ILogger logger)
    {
        _orderService = orderService;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInfo($"Handling order created message: {message.MessageId}");

        // Deserialize the message body
        var orderData = JsonSerializer.Deserialize<OrderData>(message.Body);

        // Access metadata from Properties
        var correlationId = message.Properties.GetValueOrDefault("CorrelationId");
        var eventSource = message.Properties.GetValueOrDefault("EventSource");
        
        _logger.LogInfo($"Order {orderData.OrderId}, Delivery #{message.DeliveryCount}, CorrelationId: {correlationId}");

        // Check for poison messages
        if (message.DeliveryCount > 5)
        {
            _logger.LogWarning($"Message {message.MessageId} has been retried {message.DeliveryCount} times");
        }

        // Business logic
        await _orderService.CreateOrderAsync(orderData, cancellationToken);
        await _paymentService.InitiatePaymentAsync(orderData.OrderId, orderData.Amount, cancellationToken);

        _logger.LogInfo($"Order {orderData.OrderId} processed successfully");
    }
}
```

### 3. Register in Startup

```csharp
using AppFactory.Framework.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register your platform-agnostic message handler
        services.AddScoped<IMessageHandler<OrderCreatedMessage>, OrderCreatedHandler>();

        // Register business services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        
        // Register repositories
        services.AddScoped<IOrderRepository, DynamoDbOrderRepository>();

        // Logging (required)
        services.AddSingleton<ILoggerFactory, SerilogLoggerFactory>();
        services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger());
    }
}
```

### 4. Create Lambda Function Handler

```csharp
using AppFactory.Framework.Messaging.Aws.Handlers;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace OrderService.Lambda;

/// <summary>
/// AWS Lambda function for processing order created messages from SQS
/// </summary>
public class OrderCreatedFunction : SqsMessageHandlerBase<OrderCreatedMessage>
{
    public OrderCreatedFunction() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    /// <summary>
    /// Lambda entry point
    /// </summary>
    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
        => await Handle(sqsEvent, context);
}
```

### 5. Configure AWS Infrastructure

**serverless.yml**:
```yaml
service: order-service

provider:
  name: aws
  runtime: dotnet10
  region: us-east-1
  memorySize: 512
  timeout: 30

functions:
  processOrderCreated:
    handler: OrderService.Lambda::OrderService.Lambda.OrderCreatedFunction::FunctionHandler
    events:
      - sqs:
          arn: !GetAtt OrderCreatedQueue.Arn
          batchSize: 10
          maximumBatchingWindowInSeconds: 5
          functionResponseType: ReportBatchItemFailures  # Important for partial batch failures

resources:
  Resources:
    OrderCreatedQueue:
      Type: AWS::SQS::Queue
      Properties:
        QueueName: OrderCreatedQueue
        VisibilityTimeout: 60  # Should be >= Lambda timeout
        MessageRetentionPeriod: 345600  # 4 days
        RedrivePolicy:
          deadLetterTargetArn: !GetAtt OrderCreatedDLQ.Arn
          maxReceiveCount: 3
        
    OrderCreatedDLQ:
      Type: AWS::SQS::Queue
      Properties:
        QueueName: OrderCreatedQueue-DLQ
        MessageRetentionPeriod: 1209600  # 14 days
```

## Message Metadata Mapping

All SQS attributes are mapped to `IMessage.Properties`:

| Property Key | Source | Description |
|-------------|---------|-------------|
| `MessageId` | SQS MessageId | Unique message identifier |
| `Body` | SQS Body | Message payload (JSON) |
| `DeliveryCount` | ApproximateReceiveCount | Number of delivery attempts |
| `EnqueuedTimeUtc` | SentTimestamp | When message was sent |
| `CorrelationId` | MessageAttributes | Custom correlation ID (if set by publisher) |
| `EventSource` | EventSource | "aws:sqs" |
| `EventSourceARN` | EventSourceArn | Queue ARN |
| `AWSRegion` | AwsRegion | AWS region |
| `SQS_*` | SQS Attributes | All SQS system attributes (e.g., SQS_ApproximateFirstReceiveTimestamp) |

## Advanced Scenarios

### Idempotency

```csharp
public async Task HandleAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
{
    var orderData = JsonSerializer.Deserialize<OrderData>(message.Body);
    
    // Check if already processed using MessageId
    if (await _orderRepository.ExistsAsync(orderData.OrderId, cancellationToken))
    {
        _logger.LogInfo($"Order {orderData.OrderId} already processed (MessageId: {message.MessageId}) - skipping");
        return;
    }
    
    // Process with idempotency key
    await _orderService.CreateOrderAsync(orderData, message.MessageId, cancellationToken);
}
```

### Retry Strategy

```csharp
public async Task HandleAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
{
    // Implement exponential backoff for retries
    if (message.DeliveryCount > 1)
    {
        var delayMs = Math.Min(1000 * Math.Pow(2, message.DeliveryCount - 1), 30000);
        await Task.Delay((int)delayMs, cancellationToken);
    }
    
    try
    {
        await ProcessOrder(message, cancellationToken);
    }
    catch (TransientException ex)
    {
        _logger.LogWarning($"Transient error on attempt {message.DeliveryCount}: {ex.Message}");
        throw; // Will retry
    }
    catch (PermanentException ex)
    {
        _logger.LogError(ex, "Permanent error - will move to DLQ");
        // Don't throw - message will be marked as processed
        await _deadLetterService.SendAsync(message, ex, cancellationToken);
    }
}
```

### Batch Processing

```csharp
// The base handler already supports batch processing
// Configure batch size in serverless.yml:
events:
  - sqs:
      batchSize: 10  # Process up to 10 messages at once
      maximumBatchingWindowInSeconds: 5  # Wait up to 5 seconds to fill batch
```

### FIFO Queue Support

```csharp
// For FIFO queues, message group ID is in Properties
public async Task HandleAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
{
    var messageGroupId = message.Properties.GetValueOrDefault("SQS_MessageGroupId");
    var sequenceNumber = message.Properties.GetValueOrDefault("SQS_SequenceNumber");
    
    _logger.LogInfo($"Processing FIFO message. Group: {messageGroupId}, Sequence: {sequenceNumber}");
    
    // Messages in same group are processed in order
    await ProcessInOrder(messageGroupId, message, cancellationToken);
}
```

## Testing

### Unit Testing (Handler Only)

```csharp
using Xunit;
using Moq;

public class OrderCreatedHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidMessage_CreatesOrder()
    {
        // Arrange
        var message = new OrderCreatedMessage
        {
            MessageId = "msg-123",
            Body = JsonSerializer.Serialize(new OrderData 
            { 
                OrderId = "order-123", 
                Amount = 99.99m 
            }),
            Properties = new Dictionary<string, string>
            {
                ["CorrelationId"] = "corr-123",
                ["EventSource"] = "test"
            },
            DeliveryCount = 1,
            EnqueuedTimeUtc = DateTime.UtcNow
        };

        var mockOrderService = new Mock<IOrderService>();
        var mockPaymentService = new Mock<IPaymentService>();
        var mockLogger = new Mock<ILogger>();

        var handler = new OrderCreatedHandler(
            mockOrderService.Object,
            mockPaymentService.Object,
            mockLogger.Object);

        // Act
        await handler.HandleAsync(message, CancellationToken.None);

        // Assert
        mockOrderService.Verify(
            x => x.CreateOrderAsync(
                It.Is<OrderData>(o => o.OrderId == "order-123"), 
                It.IsAny<CancellationToken>()), 
            Times.Once);
            
        mockPaymentService.Verify(
            x => x.InitiatePaymentAsync("order-123", 99.99m, It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}
```

### Integration Testing (Full Lambda)

```csharp
[Fact]
public async Task FunctionHandler_ValidSqsEvent_ProcessesSuccessfully()
{
    // Arrange
    var function = new OrderCreatedFunction();
    var sqsEvent = new SQSEvent
    {
        Records = new List<SQSEvent.SQSMessage>
        {
            new SQSEvent.SQSMessage
            {
                MessageId = "msg-123",
                Body = JsonSerializer.Serialize(new OrderData { OrderId = "order-123" }),
                MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>(),
                Attributes = new Dictionary<string, string>
                {
                    ["ApproximateReceiveCount"] = "1",
                    ["SentTimestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
                },
                EventSource = "aws:sqs",
                EventSourceArn = "arn:aws:sqs:us-east-1:123456789:OrderCreatedQueue",
                AwsRegion = "us-east-1"
            }
        }
    };
    
    var context = new TestLambdaContext
    {
        AwsRequestId = "test-request-id",
        RemainingTime = TimeSpan.FromSeconds(30)
    };

    // Act
    var response = await function.FunctionHandler(sqsEvent, context);

    // Assert
    Assert.Empty(response.BatchItemFailures);
}
```

## Error Handling

The base handler automatically:
- ✅ Catches exceptions per message
- ✅ Logs error details
- ✅ Returns failed message IDs in `SQSBatchResponse`
- ✅ Allows SQS to retry failed messages
- ✅ Sends to DLQ after max retries exceeded

## Performance Considerations

### Cold Start Optimization

```csharp
public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Use Singleton for expensive-to-create services
        services.AddSingleton<ILoggerFactory, SerilogLoggerFactory>();
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());
        
        // Use Scoped for per-message instances
        services.AddScoped<IMessageHandler<OrderCreatedMessage>, OrderCreatedHandler>();
        services.AddScoped<IOrderService, OrderService>();
    }
}
```

### Provisioned Concurrency

```yaml
functions:
  processOrderCreated:
    handler: OrderService.Lambda::OrderService.Lambda.OrderCreatedFunction::FunctionHandler
    provisionedConcurrency: 2  # Keep 2 instances warm
```

## Comparison with Other Handlers

| Feature | SqsMessageHandlerBase | LambdaMessageHandlerBase | LambdaMessageHandlerBase2 |
|---------|----------------------|-------------------------|--------------------------|
| Project | Messaging.Aws ⭐ | Messaging.Aws | Messaging (core) |
| Interface | `IMessageHandler<T>` | `IMessageHandler<T,TContext>` | `IMessageProcessor<T>` |
| Pattern | Publisher-Subscriber | Publisher-Subscriber + Context | Simple Processor |
| Cancellation | ✅ Yes | ✅ Yes | ❌ No |
| Batch Failures | ✅ Yes | ✅ Yes | ✅ Yes |
| DI Support | ✅ Full | ✅ Full | ✅ Full |
| Platform-Agnostic | ✅ Yes | ✅ Yes | ⚠️ AWS-specific |
| Message Mapping | `IMessage` | `IMessage` | Custom `Message` class |

## Migration Guide

### From LambdaMessageHandlerBase2

```diff
- using AppFactory.Framework.Messaging.LambdaHandlers;
+ using AppFactory.Framework.Messaging.Aws.Handlers;
+ using AppFactory.Framework.Messaging.Abstractions;

- public class OrderHandler : ILambdaMessageProcessor<OrderMessage>
+ public class OrderHandler : IMessageHandler<OrderMessage>
  {
-     public async Task Process(OrderMessage message)
+     public async Task HandleAsync(OrderMessage message, CancellationToken cancellationToken)
      {
          // Business logic
      }
  }

- public class Function : LambdaMessageHandlerBase2<OrderMessage>
+ public class Function : SqsMessageHandlerBase<OrderMessage>
```

## Best Practices

1. ✅ **Use `IMessageHandler<T>`** for platform-agnostic handlers
2. ✅ **Implement idempotency** using `MessageId` or business key
3. ✅ **Handle transient vs permanent errors** differently
4. ✅ **Set appropriate visibility timeout** (>= Lambda timeout)
5. ✅ **Configure DLQ** for poison messages
6. ✅ **Use batch processing** for better throughput
7. ✅ **Monitor CloudWatch metrics** (ApproximateAgeOfOldestMessage, NumberOfMessagesSent)
8. ✅ **Implement retry strategy** with exponential backoff
9. ✅ **Use cancellation tokens** to respect Lambda timeout

## Related Documentation

- [Messaging.Core Abstractions](../../AppFactory.Framework.Messaging.Core/README.md)
- [Message Publishing](../SqsMessagePublisher.cs)
- [Azure Service Bus Handler](../../AppFactory.Framework.Messaging.Azure/FunctionHandlers/README.md)
