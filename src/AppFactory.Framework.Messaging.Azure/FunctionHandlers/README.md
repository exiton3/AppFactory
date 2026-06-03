# Azure Functions Message Handlers

Base classes for Azure Functions that handle messages from Azure Service Bus and Azure Storage Queues, following the same pattern as `LambdaMessageHandlerBase2` for AWS Lambda.

## 📋 Overview

These handler base classes provide:
- **Dependency Injection** with `IStartup` pattern
- **Message Processor Pattern** using `ILambdaMessageProcessor<TMessage>`
- **Automatic Message Mapping** to custom `Message` types
- **Error Handling** with proper exception propagation
- **Performance Logging** for message processing
- **Batch Processing Support**

## 🚀 Usage

### Service Bus Queue Handler

```csharp
using AppFactory.Framework.Messaging.Azure.FunctionHandlers;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;

public class OrderMessageHandler : ServiceBusFunctionHandlerBase<OrderMessage>
{
    public OrderMessageHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    [Function("ProcessOrderMessage")]
    public async Task Run(
        [ServiceBusTrigger("%OrderQueueName%", Connection = "ServiceBusConnection")] 
        ServiceBusReceivedMessage message,
        FunctionContext context)
    {
        await Handle(message, context);
    }
}
```

### Service Bus Topic Handler

```csharp
public class OrderEventHandler : ServiceBusFunctionHandlerBase<OrderEvent>
{
    public OrderEventHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    [Function("ProcessOrderEvent")]
    public async Task Run(
        [ServiceBusTrigger("%OrderTopicName%", "%SubscriptionName%", Connection = "ServiceBusConnection")] 
        ServiceBusReceivedMessage message,
        FunctionContext context)
    {
        await HandleTopicMessage(message, context);
    }
}
```

### Service Bus Batch Handler

```csharp
public class OrderBatchHandler : ServiceBusFunctionHandlerBase<OrderMessage>
{
    public OrderBatchHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    [Function("ProcessOrderBatch")]
    public async Task Run(
        [ServiceBusTrigger("%OrderQueueName%", Connection = "ServiceBusConnection")] 
        ServiceBusReceivedMessage[] messages,
        FunctionContext context)
    {
        await HandleBatch(messages, context);
    }
}
```

### Queue Storage Handler

```csharp
using AppFactory.Framework.Messaging.Azure.FunctionHandlers;
using Microsoft.Azure.Functions.Worker;
using Azure.Storage.Queues.Models;

public class NotificationHandler : QueueStorageFunctionHandlerBase<NotificationMessage>
{
    public NotificationHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    [Function("ProcessNotification")]
    public async Task Run(
        [QueueTrigger("%NotificationQueueName%", Connection = "AzureWebJobsStorage")] 
        QueueMessage message,
        FunctionContext context)
    {
        await Handle(message, context);
    }
}
```

### Queue Storage String Handler

```csharp
public class SimpleMessageHandler : QueueStorageFunctionHandlerBase<SimpleMessage>
{
    public SimpleMessageHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    [Function("ProcessSimpleMessage")]
    public async Task Run(
        [QueueTrigger("%QueueName%", Connection = "AzureWebJobsStorage")] 
        string messageBody,
        FunctionContext context)
    {
        await HandleString(messageBody, context);
    }
}
```

## 📦 Message Type

Your message class must inherit from `Message`:

```csharp
public class OrderMessage : Message
{
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CustomerId { get; set; }
}
```

## 🔧 Message Processor

Implement `ILambdaMessageProcessor<TMessage>` to process your messages:

```csharp
public class OrderMessageProcessor : ILambdaMessageProcessor<OrderMessage>
{
    private readonly IOrderService _orderService;
    private readonly ILogger _logger;

    public OrderMessageProcessor(IOrderService orderService, ILogger logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task Process(OrderMessage message)
    {
        _logger.LogInfo($"Processing order {message.OrderId}");
        
        // Deserialize the body
        var orderData = JsonSerializer.Deserialize<OrderData>(message.Body);
        
        // Process the order
        await _orderService.ProcessOrderAsync(orderData);
        
        _logger.LogInfo($"Order {message.OrderId} processed successfully");
    }
}
```

## ⚙️ Startup Configuration

Configure your dependencies using `IStartup`:

```csharp
public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register your message processor
        services.AddScoped<ILambdaMessageProcessor<OrderMessage>, OrderMessageProcessor>();
        
        // Register your services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        // Add logging
        services.AddSingleton<ILoggerFactory, MicrosoftExtensionsLoggerFactory>();
        services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger());
    }
}
```

## 🔄 Error Handling

### Service Bus
- **Single Message**: Exceptions are thrown and the message is moved to the Dead Letter Queue (if configured)
- **Batch**: Individual message failures are logged; aggregate exception is thrown if any message fails

### Queue Storage
- **Single Message**: Exceptions are thrown and the message is retried based on `MaxDequeueCount` setting
- **After Max Retries**: Message is moved to poison queue (queuename-poison)
- **Batch**: Individual message failures are logged; aggregate exception is thrown if any message fails

## 📝 Configuration

### local.settings.json

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBusConnection": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key",
    "OrderQueueName": "orders",
    "OrderTopicName": "order-events",
    "SubscriptionName": "order-processor",
    "NotificationQueueName": "notifications"
  }
}
```

### host.json

```json
{
  "version": "2.0",
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true
      }
    }
  },
  "extensions": {
    "serviceBus": {
      "prefetchCount": 100,
      "messageHandlerOptions": {
        "autoComplete": true,
        "maxConcurrentCalls": 32,
        "maxAutoRenewDuration": "00:05:00"
      },
      "sessionHandlerOptions": {
        "autoComplete": false,
        "messageWaitTimeout": "00:00:30",
        "maxAutoRenewDuration": "00:55:00",
        "maxConcurrentSessions": 16
      }
    },
    "queues": {
      "maxPollingInterval": "00:00:02",
      "visibilityTimeout": "00:00:30",
      "batchSize": 16,
      "maxDequeueCount": 5,
      "newBatchThreshold": 8
    }
  }
}
```

## 🎯 Key Features

### Correlation Tracking
- Service Bus: Uses `CorrelationId` from message or function `InvocationId`
- Queue Storage: Uses function `InvocationId`

### Message Attributes
- **Service Bus**: Captures `ApplicationProperties`, `CorrelationId`, `SessionId`, `DeliveryCount`, `EnqueuedTimeUtc`
- **Queue Storage**: Captures `DequeueCount`, `InsertedOn`, `ExpiresOn`, `NextVisibleOn`, `PopReceipt`

### Performance Logging
All message processing is wrapped with performance logging to track execution time.

## 🔗 Related

- [AWS Lambda Handler](../../AppFactory.Framework.Messaging/LambdaHandlers/README.md)
- [Service Bus Publisher](../ServiceBusMessagePublisher.cs)
- [Queue Storage Publisher](../QueueStorageMessagePublisher.cs)
