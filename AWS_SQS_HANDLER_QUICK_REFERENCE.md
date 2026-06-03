# AWS SQS Handler - Quick Reference

## 🎯 Publisher-Subscriber Pattern

```
Your Handler (Business Logic)
        ↓
IMessageHandler<TMessage>
        ↓
SqsMessageHandlerBase (AWS Integration)
        ↓
   Amazon SQS Queue
```

## 📝 Quick Start (4 Steps)

### 1. Define Message
```csharp
using AppFactory.Framework.Messaging.Abstractions;

public class OrderMessage : Message
{
    // Inherits: MessageId, Body, Properties, EnqueuedTimeUtc, DeliveryCount
}
```

### 2. Implement Handler
```csharp
using AppFactory.Framework.Messaging.Abstractions;

public class OrderHandler : IMessageHandler<OrderMessage>
{
    private readonly IOrderService _orderService;
    private readonly ILogger _logger;

    public OrderHandler(IOrderService orderService, ILogger logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task HandleAsync(OrderMessage message, CancellationToken ct)
    {
        var data = JsonSerializer.Deserialize<OrderData>(message.Body);
        await _orderService.ProcessAsync(data, ct);
    }
}
```

### 3. Register in Startup
```csharp
public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IMessageHandler<OrderMessage>, OrderHandler>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddSerilogLogging();
    }
}
```

### 4. Create Lambda Function
```csharp
using AppFactory.Framework.Messaging.Aws.Handlers;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

public class OrderFunction : SqsMessageHandlerBase<OrderMessage>
{
    public OrderFunction() : base(new Startup()) { }
    protected override IStartup GetStartup() => new Startup();
    
    public Task<SQSBatchResponse> FunctionHandler(SQSEvent e, ILambdaContext c)
        => Handle(e, c);
}
```

## 📦 Message Structure

```csharp
public class OrderMessage : Message
{
    // Auto-populated by base handler:
    // - string MessageId (from SQS)
    // - string Body (JSON payload)
    // - Dictionary<string, string> Properties (all SQS attributes)
    // - DateTime EnqueuedTimeUtc (SentTimestamp)
    // - int DeliveryCount (ApproximateReceiveCount)
}
```

## 🔑 Access Metadata

```csharp
public async Task HandleAsync(OrderMessage message, CancellationToken ct)
{
    // Basic properties
    var msgId = message.MessageId;
    var body = message.Body;
    var deliveryCount = message.DeliveryCount;
    var enqueuedTime = message.EnqueuedTimeUtc;
    
    // Custom attributes
    var correlationId = message.Properties.GetValueOrDefault("CorrelationId");
    
    // SQS system attributes
    var eventSource = message.Properties.GetValueOrDefault("EventSource");
    var queueArn = message.Properties.GetValueOrDefault("EventSourceARN");
    var region = message.Properties.GetValueOrDefault("AWSRegion");
    
    // FIFO queue attributes (if applicable)
    var messageGroupId = message.Properties.GetValueOrDefault("SQS_MessageGroupId");
    var sequenceNumber = message.Properties.GetValueOrDefault("SQS_SequenceNumber");
}
```

## ⚙️ serverless.yml Configuration

```yaml
service: order-service

provider:
  name: aws
  runtime: dotnet10
  region: us-east-1

functions:
  processOrders:
    handler: OrderService::OrderService.OrderFunction::FunctionHandler
    events:
      - sqs:
          arn: !GetAtt OrderQueue.Arn
          batchSize: 10
          functionResponseType: ReportBatchItemFailures

resources:
  Resources:
    OrderQueue:
      Type: AWS::SQS::Queue
      Properties:
        QueueName: OrderQueue
        VisibilityTimeout: 60
        RedrivePolicy:
          deadLetterTargetArn: !GetAtt OrderDLQ.Arn
          maxReceiveCount: 3
    
    OrderDLQ:
      Type: AWS::SQS::Queue
      Properties:
        QueueName: OrderQueue-DLQ
```

## ✅ Common Patterns

### Idempotency
```csharp
public async Task HandleAsync(OrderMessage message, CancellationToken ct)
{
    if (await _repo.ExistsAsync(message.MessageId, ct))
    {
        _logger.LogInfo($"Already processed {message.MessageId}");
        return;
    }
    
    await ProcessOrder(message, ct);
}
```

### Retry Logic
```csharp
public async Task HandleAsync(OrderMessage message, CancellationToken ct)
{
    try
    {
        await ProcessOrder(message, ct);
    }
    catch (TransientException ex)
    {
        _logger.LogWarning($"Retry {message.DeliveryCount}: {ex.Message}");
        throw; // Will retry
    }
    catch (PermanentException ex)
    {
        _logger.LogError(ex, "Permanent error");
        // Don't throw - won't retry
    }
}
```

### Poison Message Detection
```csharp
public async Task HandleAsync(OrderMessage message, CancellationToken ct)
{
    if (message.DeliveryCount > 5)
    {
        _logger.LogWarning($"Message {message.MessageId} retried {message.DeliveryCount} times");
        // Send to custom DLQ or alert
    }
    
    await ProcessOrder(message, ct);
}
```

## 🧪 Unit Testing

```csharp
[Fact]
public async Task HandleAsync_ValidMessage_ProcessesOrder()
{
    // Arrange
    var message = new OrderMessage
    {
        MessageId = "test-123",
        Body = JsonSerializer.Serialize(new { OrderId = "order-123" }),
        DeliveryCount = 1,
        EnqueuedTimeUtc = DateTime.UtcNow
    };
    
    var mockService = new Mock<IOrderService>();
    var handler = new OrderHandler(mockService.Object, Mock.Of<ILogger>());
    
    // Act
    await handler.HandleAsync(message, CancellationToken.None);
    
    // Assert
    mockService.Verify(x => x.ProcessAsync(It.IsAny<OrderData>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

## 🚀 Deploy

```bash
# Build
dotnet build

# Deploy with Serverless Framework
serverless deploy --stage dev

# Or with AWS SAM
sam build
sam deploy --guided
```

## 📊 Message Properties Mapping

| Property | Source | Description |
|----------|--------|-------------|
| `MessageId` | SQS MessageId | Unique identifier |
| `Body` | SQS Body | JSON payload |
| `DeliveryCount` | ApproximateReceiveCount | Retry count |
| `EnqueuedTimeUtc` | SentTimestamp | When sent |
| `EventSource` | EventSource | "aws:sqs" |
| `EventSourceARN` | EventSourceArn | Queue ARN |
| `AWSRegion` | AwsRegion | Region |
| `SQS_*` | All SQS attributes | System metadata |

## ⚡ Key Features

✅ Platform-agnostic handler (`IMessageHandler<T>`)  
✅ Automatic SQS → Message mapping  
✅ Batch failure handling  
✅ DI scope per message  
✅ Performance logging  
✅ Cancellation token support  
✅ All SQS metadata accessible  
✅ Easy unit testing (no AWS dependencies)

## 📚 Full Documentation

- **Complete Guide**: `src\AppFactory.Framework.Messaging.Aws\Handlers\README.md`
- **Examples**: `src\AppFactory.Framework.Messaging.Aws\Handlers\EXAMPLES.md`
- **Summary**: `AWS_SQS_HANDLER_IMPLEMENTATION_SUMMARY.md`

## 🔄 Migration from LambdaMessageHandlerBase2

```diff
- using AppFactory.Framework.Messaging.LambdaHandlers;
+ using AppFactory.Framework.Messaging.Aws.Handlers;
+ using AppFactory.Framework.Messaging.Abstractions;

- public class MyHandler : ILambdaMessageProcessor<MyMessage>
+ public class MyHandler : IMessageHandler<MyMessage>
{
-   public async Task Process(MyMessage message)
+   public async Task HandleAsync(MyMessage message, CancellationToken ct)
    {
        // Business logic
    }
}

- public class Function : LambdaMessageHandlerBase2<MyMessage>
+ public class Function : SqsMessageHandlerBase<MyMessage>

- services.AddScoped<ILambdaMessageProcessor<MyMessage>, MyHandler>();
+ services.AddScoped<IMessageHandler<MyMessage>, MyHandler>();
```

## 🎯 Best Practices Checklist

- [ ] Implement idempotency using `MessageId`
- [ ] Handle transient vs permanent errors differently
- [ ] Use cancellation token for long operations
- [ ] Set visibility timeout >= Lambda timeout
- [ ] Configure Dead Letter Queue
- [ ] Monitor CloudWatch metrics
- [ ] Use batch processing (batchSize > 1)
- [ ] Implement retry with exponential backoff
- [ ] Log correlation IDs for tracing
- [ ] Test handlers without AWS dependencies
