# AppFactory.Framework.Messaging.Aws

**AWS SQS messaging implementation for building reactive microservices on AWS Lambda.**

[![NuGet](https://img.shields.io/nuget/v/AppFactory.Framework.Messaging.Aws.svg)](https://www.nuget.org/packages/AppFactory.Framework.Messaging.Aws/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## 📋 Overview

`AppFactory.Framework.Messaging.Aws` provides **AWS SQS-specific** implementations of the platform-agnostic messaging abstractions from `AppFactory.Framework.Messaging.Core`. Build reactive, event-driven microservices on AWS Lambda with queue-based messaging.

### Key Features

✅ **AWS SQS Integration** - Native SQS message publishing and consumption  
✅ **Lambda Handler Base Classes** - Simplified Lambda function development  
✅ **Automatic Deserialization** - Type-safe message handling  
✅ **Dead Letter Queue Support** - Automatic DLQ integration  
✅ **Batch Publishing** - Efficient batch operations (up to 10 messages)  
✅ **Correlation Tracking** - Built-in distributed tracing support  
✅ **Context-Based Handling** - Complete/Abandon/DeadLetter operations  

---

## 🚀 Installation

```bash
dotnet add package AppFactory.Framework.Messaging.Aws --version 10.5.0
```

**Dependencies:**
```bash
dotnet add package AppFactory.Framework.Messaging.Core --version 10.5.0
```

---

## 💡 Quick Start

### 1. Configure Services

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add AWS SQS messaging
        services.AddAwsMessaging(options =>
        {
            options.QueueUrl = Environment.GetEnvironmentVariable("QUEUE_URL");
            options.DeadLetterQueueUrl = Environment.GetEnvironmentVariable("DLQ_URL");
            options.MaxRetries = 3;
            options.EnableDetailedLogging = true;
        });

        // Or use configuration
        services.AddAwsMessaging(Configuration.GetSection("AwsSqs"));
    }
}
```

**appsettings.json:**
```json
{
  "AwsSqs": {
    "QueueUrl": "https://sqs.us-east-1.amazonaws.com/123456789/my-queue",
    "DeadLetterQueueUrl": "https://sqs.us-east-1.amazonaws.com/123456789/my-queue-dlq",
    "MaxRetries": 3,
    "DelaySeconds": 0,
    "MaxBatchSize": 10,
    "EnableDetailedLogging": false
  }
}
```

### 2. Publish Messages

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

        // Publish message to SQS
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

### 3. Handle Messages (Simple Lambda Handler)

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
        CancellationToken cancellationToken)
    {
        Logger.LogInformation($"Sending confirmation email for order {message.OrderId}");
        await _emailService.SendOrderConfirmationAsync(message.OrderId);
    }
}
```

### 4. Handle Messages (Context-Based Handler)

```csharp
public class ProcessPaymentFunction : LambdaMessageHandlerWithContextBase<OrderCreatedMessage>
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
            
            // Explicitly mark as complete (auto-deleted from queue)
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
            
            // Retry later (returns message to queue)
            await context.AbandonAsync(cancellationToken);
        }
    }
}
```

---

## 📦 Lambda Function Setup

### serverless.yml Configuration

```yaml
service: order-service

provider:
  name: aws
  runtime: dotnet8
  region: us-east-1
  environment:
    QUEUE_URL: !Ref OrderQueue

functions:
  processOrder:
    handler: OrderService::OrderService.ProcessOrderFunction::FunctionHandler
    events:
      - sqs:
          arn: !GetAtt OrderQueue.Arn
          batchSize: 10
          maximumBatchingWindowInSeconds: 5

resources:
  Resources:
    OrderQueue:
      Type: AWS::SQS::Queue
      Properties:
        QueueName: order-queue
        VisibilityTimeout: 300
        RedrivePolicy:
          deadLetterTargetArn: !GetAtt OrderDLQ.Arn
          maxReceiveCount: 3

    OrderDLQ:
      Type: AWS::SQS::Queue
      Properties:
        QueueName: order-queue-dlq
        MessageRetentionPeriod: 1209600 # 14 days
```

### Lambda Function Handler

```csharp
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace OrderService;

public class ProcessOrderFunction : LambdaMessageHandlerBase<OrderCreatedMessage>
{
    public ProcessOrderFunction() : base(CreateLogger())
    {
    }

    private static ILogger CreateLogger()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ILogger>();
    }

    protected override async Task HandleMessageAsync(
        OrderCreatedMessage message, 
        CancellationToken cancellationToken)
    {
        Logger.LogInformation($"Processing order: {message.OrderId}");
        // Business logic here
    }
}
```

---

## 🔄 Batch Publishing

Publish multiple messages efficiently:

```csharp
public class BulkOrderProcessor
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

        // Publish up to 10 messages per batch
        var result = await _publisher.PublishBatchAsync(messages);

        if (!result.IsSuccess)
        {
            foreach (var failure in result.Results.Where(r => !r.IsSuccess))
            {
                _logger.LogError($"Failed: {failure.ErrorMessage}");
            }
        }

        _logger.LogInformation($"Published {result.SuccessCount} messages, {result.FailureCount} failed");
    }
}
```

---

## 📊 Message Attributes & Correlation Tracking

SQS message attributes are automatically populated:

```csharp
var message = new OrderCreatedMessage { ... };

// Add correlation tracking
message.AddCorrelationId(correlationId);
message.AddCausationId(previousMessageId);
message.AddUserId(currentUserId);

await _publisher.PublishAsync(message);

// In Lambda handler, metadata is automatically populated:
protected override async Task HandleMessageAsync(OrderCreatedMessage message, ...)
{
    var correlationId = message.Properties["CorrelationId"];
    var messageId = message.MessageId;
    var deliveryCount = message.DeliveryCount;
    var enqueuedTime = message.EnqueuedTimeUtc;
    
    Logger.LogInformation($"[{correlationId}] Processing message {messageId}, Attempt: {deliveryCount}");
}
```

---

## ⚙️ Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `QueueUrl` | string | *Required* | SQS queue URL for publishing |
| `DeadLetterQueueUrl` | string? | null | DLQ URL for failed messages |
| `DelaySeconds` | int | 0 | Message delay (0-900 seconds) |
| `MaxRetries` | int | 3 | Retry attempts for publish failures |
| `Region` | string? | null | AWS region (uses SDK default if null) |
| `EnableDetailedLogging` | bool | false | Enable debug-level logging |
| `MaxBatchSize` | int | 10 | Max messages per batch (1-10) |

---

## 🔒 Error Handling & Dead Letter Queues

### Automatic DLQ with Lambda

```csharp
// Configure max retries in SQS queue
resources:
  Resources:
    OrderQueue:
      Type: AWS::SQS::Queue
      Properties:
        RedrivePolicy:
          deadLetterTargetArn: !GetAtt OrderDLQ.Arn
          maxReceiveCount: 3  # Move to DLQ after 3 failures
```

### Explicit Dead Lettering

```csharp
protected override async Task HandleMessageAsync(
    OrderCreatedMessage message,
    IMessageContext context,
    CancellationToken cancellationToken)
{
    if (message.TotalAmount > 10000)
    {
        // Invalid message - don't retry
        await context.DeadLetterAsync("Amount exceeds limit");
        return;
    }

    try
    {
        await ProcessOrderAsync(message);
        await context.CompleteAsync();
    }
    catch (Exception ex)
    {
        // Transient error - retry
        await context.AbandonAsync();
    }
}
```

---

## 🧪 Testing

### Mock SQS Publisher

```csharp
[Fact]
public async Task CreateOrder_ShouldPublishMessageToSqs()
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

### Integration Testing with LocalStack

```bash
# Start LocalStack with SQS
docker run -d -p 4566:4566 localstack/localstack

# Create test queue
aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name test-queue
```

```csharp
[Fact]
public async Task PublishAsync_ShouldSendMessageToLocalStack()
{
    var services = new ServiceCollection();
    services.AddAwsMessaging(options =>
    {
        options.QueueUrl = "http://localhost:4566/000000000000/test-queue";
    });

    var publisher = services.BuildServiceProvider().GetRequiredService<IMessagePublisher>();
    var result = await publisher.PublishAsync(new TestMessage { Data = "test" });

    result.IsSuccess.ShouldBe(true);
}
```

---

## 🌐 Multi-Cloud Support

Use the same business logic across clouds:

```csharp
// This interface works on AWS SQS, Azure Service Bus, etc.
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

**Switch to Azure:**
```csharp
// Just change registration - business logic unchanged
services.AddAzureMessaging(options => { ... }); // Instead of AddAwsMessaging
```

---

## 📚 Related Packages

| Package | Purpose |
|---------|---------|
| **AppFactory.Framework.Messaging.Core** | Platform-agnostic abstractions |
| **AppFactory.Framework.Messaging.Aws** | AWS SQS implementation (this package) |
| **AppFactory.Framework.Messaging.Azure** | Azure Service Bus + Queue Storage |
| **AppFactory.Framework.EventBus.Aws** | AWS EventBridge (pub/sub events) |

---

## 🔗 Resources

- [AWS SQS Documentation](https://docs.aws.amazon.com/sqs/)
- [Lambda SQS Integration](https://docs.aws.amazon.com/lambda/latest/dg/with-sqs.html)
- [Messaging.Core Package](../AppFactory.Framework.Messaging.Core/README.md)
- [GitHub Repository](https://github.com/exiton3/AppFactory)

---

**AppFactory.Framework.Messaging.Aws** - Build Reactive Microservices on AWS! 🚀
