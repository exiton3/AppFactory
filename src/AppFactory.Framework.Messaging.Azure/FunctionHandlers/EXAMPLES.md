# Azure Functions Message Handler Examples

Complete examples showing how to use the Azure Functions message handlers following the same pattern as `LambdaMessageHandlerBase2`.

## 📦 Sample Message Types

```csharp
using AppFactory.Framework.Messaging.LambdaHandlers;

public class OrderCreatedMessage : Message
{
    // Message inherits Body, MessageId, Source, Attributes
}

public class OrderUpdatedMessage : Message
{
}

public class NotificationMessage : Message
{
}
```

## 🔧 Sample Message Processors

```csharp
using AppFactory.Framework.Logging;
using AppFactory.Framework.Messaging.LambdaHandlers;
using System.Text.Json;

public class OrderCreatedMessageProcessor : ILambdaMessageProcessor<OrderCreatedMessage>
{
    private readonly IOrderService _orderService;
    private readonly ILogger _logger;

    public OrderCreatedMessageProcessor(IOrderService orderService, ILogger logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task Process(OrderCreatedMessage message)
    {
        _logger.LogInfo($"Processing OrderCreated message {message.MessageId}");
        
        // Deserialize the message body
        var orderData = JsonSerializer.Deserialize<OrderData>(message.Body);
        
        // Get correlation ID from attributes
        var correlationId = message.Attributes.GetValueOrDefault("CorrelationId");
        
        // Process the order
        await _orderService.CreateOrderAsync(orderData, correlationId);
        
        _logger.LogInfo($"OrderCreated message {message.MessageId} processed successfully");
    }
}

public class NotificationMessageProcessor : ILambdaMessageProcessor<NotificationMessage>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger _logger;

    public NotificationMessageProcessor(INotificationService notificationService, ILogger logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Process(NotificationMessage message)
    {
        _logger.LogInfo($"Processing Notification message {message.MessageId}");
        
        var notificationData = JsonSerializer.Deserialize<NotificationData>(message.Body);
        await _notificationService.SendAsync(notificationData);
        
        _logger.LogInfo($"Notification message {message.MessageId} sent successfully");
    }
}
```

## ⚙️ Startup Configuration

```csharp
using AppFactory.Framework.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Logging.MicrosoftExtensions;

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register logging
        services.AddSingleton<ILoggerFactory, MicrosoftExtensionsLoggerFactory>();
        services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger());

        // Register message processors
        services.AddScoped<ILambdaMessageProcessor<OrderCreatedMessage>, OrderCreatedMessageProcessor>();
        services.AddScoped<ILambdaMessageProcessor<OrderUpdatedMessage>, OrderUpdatedMessageProcessor>();
        services.AddScoped<ILambdaMessageProcessor<NotificationMessage>, NotificationMessageProcessor>();
        
        // Register business services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        
        // Add other dependencies
        services.AddHttpClient();
    }
}
```

## 🚀 Azure Function Handlers

### Example 1: Service Bus Queue Handler

```csharp
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;
using AppFactory.Framework.Messaging.Azure.FunctionHandlers;

namespace OrderService.Functions;

public class OrderCreatedHandler : ServiceBusFunctionHandlerBase<OrderCreatedMessage>
{
    // Constructor initializes the base with Startup configuration
    public OrderCreatedHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    [Function("ProcessOrderCreated")]
    public async Task Run(
        [ServiceBusTrigger("%OrderCreatedQueueName%", Connection = "ServiceBusConnection")] 
        ServiceBusReceivedMessage message,
        FunctionContext context)
    {
        await Handle(message, context);
    }
}
```

### Example 2: Service Bus Topic Handler

```csharp
public class OrderEventHandler : ServiceBusFunctionHandlerBase<OrderUpdatedMessage>
{
    public OrderEventHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    [Function("ProcessOrderUpdated")]
    public async Task Run(
        [ServiceBusTrigger(
            "%OrderTopicName%", 
            "%OrderSubscriptionName%", 
            Connection = "ServiceBusConnection")] 
        ServiceBusReceivedMessage message,
        FunctionContext context)
    {
        await HandleTopicMessage(message, context);
    }
}
```

### Example 3: Service Bus Batch Processing

```csharp
public class OrderBatchHandler : ServiceBusFunctionHandlerBase<OrderCreatedMessage>
{
    public OrderBatchHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    [Function("ProcessOrderBatch")]
    public async Task Run(
        [ServiceBusTrigger("%OrderQueueName%", Connection = "ServiceBusConnection", IsBatched = true)] 
        ServiceBusReceivedMessage[] messages,
        FunctionContext context)
    {
        await HandleBatch(messages, context);
    }
}
```

### Example 4: Queue Storage Handler

```csharp
using Microsoft.Azure.Functions.Worker;
using Azure.Storage.Queues.Models;
using AppFactory.Framework.Messaging.Azure.FunctionHandlers;

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

### Example 5: Queue Storage String Handler (Simple)

```csharp
public class SimpleNotificationHandler : QueueStorageFunctionHandlerBase<NotificationMessage>
{
    public SimpleNotificationHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    [Function("ProcessSimpleNotification")]
    public async Task Run(
        [QueueTrigger("%SimpleQueueName%", Connection = "AzureWebJobsStorage")] 
        string messageBody,
        FunctionContext context)
    {
        await HandleString(messageBody, context);
    }
}
```

### Example 6: Queue Storage Batch Processing

```csharp
public class NotificationBatchHandler : QueueStorageFunctionHandlerBase<NotificationMessage>
{
    public NotificationBatchHandler() : base(new Startup())
    {
    }

    protected override IStartup GetStartup() => new Startup();

    [Function("ProcessNotificationBatch")]
    public async Task Run(
        [QueueTrigger("%NotificationQueueName%", Connection = "AzureWebJobsStorage")] 
        QueueMessage[] messages,
        FunctionContext context)
    {
        await HandleBatch(messages, context);
    }
}
```

## 📝 Configuration Files

### local.settings.json

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    
    "ServiceBusConnection": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key",
    
    "OrderCreatedQueueName": "order-created",
    "OrderQueueName": "orders",
    "OrderTopicName": "order-events",
    "OrderSubscriptionName": "order-processor",
    
    "NotificationQueueName": "notifications",
    "SimpleQueueName": "simple-notifications"
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
        "isEnabled": true,
        "excludedTypes": "Request"
      }
    },
    "logLevel": {
      "default": "Information",
      "Host": "Information",
      "Function": "Information"
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
      },
      "batchOptions": {
        "maxMessageCount": 100,
        "operationTimeout": "00:01:00",
        "autoComplete": true
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

### .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AzureFunctionsVersion>v4</AzureFunctionsVersion>
    <OutputType>Exe</OutputType>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.Azure.Functions.Worker" Version="2.0.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="2.0.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.ServiceBus" Version="5.16.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Storage.Queues" Version="5.3.0" />
    <PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="2.22.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.ApplicationInsights" Version="1.2.0" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\..\src\AppFactory.Framework.Messaging.Azure\AppFactory.Framework.Messaging.Azure.csproj" />
    <ProjectReference Include="..\..\src\AppFactory.Framework.Logging.MicrosoftExtensions\AppFactory.Framework.Logging.MicrosoftExtensions.csproj" />
  </ItemGroup>
  
  <ItemGroup>
    <None Update="host.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="local.settings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <CopyToPublishDirectory>Never</CopyToPublishDirectory>
    </None>
  </ItemGroup>
</Project>
```

### Program.cs

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
```

## 🎯 Key Differences from AWS Lambda

| Feature | AWS Lambda (SQS) | Azure Functions (Service Bus) | Azure Functions (Queue Storage) |
|---------|------------------|-------------------------------|--------------------------------|
| **Batch Response** | `SQSBatchResponse` with `BatchItemFailure` list | Throw exception for individual failures | Throw exception for individual failures |
| **Message Attributes** | `MessageAttributes` | `ApplicationProperties` | Encoded in message body |
| **Dead Letter** | Configured DLQ | Dead Letter Queue/Topic | Poison queue (automatic) |
| **Sessions** | N/A | Supported via `SessionId` | N/A |
| **Correlation** | Via attributes | `CorrelationId` property | Via message content |
| **Max Batch Size** | 10 messages | 100 messages (configurable) | 32 messages (configurable) |

## 🔄 Migration from AWS Lambda

If you're migrating from AWS Lambda to Azure Functions:

1. **Replace** `LambdaMessageHandlerBase2<TMessage>` with:
   - `ServiceBusFunctionHandlerBase<TMessage>` for Service Bus
   - `QueueStorageFunctionHandlerBase<TMessage>` for Queue Storage

2. **Change** the function signature from:
   ```csharp
   public async Task<SQSBatchResponse> Handle(SQSEvent @event, ILambdaContext context)
   ```
   To:
   ```csharp
   public async Task Run([ServiceBusTrigger(...)] ServiceBusReceivedMessage message, FunctionContext context)
   ```

3. **Keep** the same:
   - `IStartup` configuration
   - `ILambdaMessageProcessor<TMessage>` implementation
   - Message types inheriting from `Message`
   - Dependency injection setup

4. **Update** configuration from environment variables to `local.settings.json`

5. **Note**: Azure Functions use different error handling (exceptions vs batch response)

## 📚 See Also

- [AWS Lambda Handler Documentation](../../../AppFactory.Framework.Messaging/LambdaHandlers/README.md)
- [Service Bus Documentation](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
- [Queue Storage Documentation](https://learn.microsoft.com/en-us/azure/storage/queues/)
- [Azure Functions Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/)
