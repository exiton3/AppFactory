# AppFactory.Framework.Messaging.Azure

**Azure Service Bus and Queue Storage messaging for Azure Functions (isolated worker model).**

[![NuGet](https://img.shields.io/nuget/v/AppFactory.Framework.Messaging.Azure.svg)](https://www.nuget.org/packages/AppFactory.Framework.Messaging.Azure/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## Overview

`AppFactory.Framework.Messaging.Azure` provides Azure-specific implementations of the platform-agnostic messaging abstractions from `AppFactory.Framework.Messaging.Core`. Targets the **Azure Functions isolated worker model** hosted in Azure Container Apps (or standalone).

### What's New in 10.7.0

- **`ServiceBusEventRouterBase`** — base class for single-subscription / multi-event-type Service Bus functions. Reads `ApplicationProperties["EventType"]`, dispatches to the registered handler, and handles Complete / Abandon / DeadLetter settlement.
- **`AddServiceBusEventHandler<TEvent, THandler>()`** — DI extension that wires an event type string to a handler. Uses a pre-compiled delegate (no reflection at message-processing time) and an O(1) property cache for hydration.
- **`IServiceBusEventHandler<TEvent>`** — typed handler contract for topic events. Settlement is managed by the router — do not call Complete/Abandon from within the handler.
- **Publisher fix** — `ServiceBusMessagePublisher` and `QueueStorageMessagePublisher` both use `ICorrelatedEnvelope` for a single publish path. `CorrelatedEvent` and `CorrelatedMessage` are handled with no type-dispatch chain.

---

## Installation

```bash
dotnet add package AppFactory.Framework.Messaging.Azure --version 10.7.0
dotnet add package AppFactory.Framework.Messaging.Core  --version 10.7.0
```

---

## Topic Event Routing (recommended for topics)

Use `ServiceBusEventRouterBase` when one subscription receives multiple event types distinguished by `ApplicationProperties["EventType"]`.

### 1. Define events

```csharp
// Domain base class — keeps TenantId/UserId out of the framework
public abstract class TenantEvent : CorrelatedEvent
{
    public string TenantId { get; set; } = default!;
    public string UserId   { get; set; } = default!;

    public override Dictionary<string, string> GetApplicationProperties()
        => new() { ["TenantId"] = TenantId, ["UserId"] = UserId };
}

public class OrderCreatedEvent : TenantEvent
{
    public override string EventType => "OrderCreated";
    public string OrderId { get; set; } = default!;
}

public class OrderShippedEvent : TenantEvent
{
    public override string EventType => "OrderShipped";
    public string TrackingNumber { get; set; } = default!;
}
```

### 2. Implement handlers

```csharp
public class OrderCreatedEventHandler : IServiceBusEventHandler<OrderCreatedEvent>
{
    private readonly ICommandDispatcher _dispatcher;

    public OrderCreatedEventHandler(ICommandDispatcher dispatcher)
        => _dispatcher = dispatcher;

    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // TenantId, UserId, OrderId are hydrated automatically from ApplicationProperties
        var result = await _dispatcher.Dispatch(
            new CreateOrderCommand { OrderId = @event.OrderId, TenantId = @event.TenantId },
            cancellationToken);

        if (!result.IsSuccess)
            throw new InvalidOperationException($"CreateOrder failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        // Throw on failure — router Abandons the message; it will be retried or dead-lettered
    }
}
```

### 3. Create the router function

```csharp
public class OrderEventRouterFunction : ServiceBusEventRouterBase
{
    public OrderEventRouterFunction(
        IServiceScopeFactory scopeFactory,
        ILogger logger,
        IEnumerable<EventHandlerRegistration> registrations)
        : base(scopeFactory, logger, registrations) { }

    [Function(nameof(OrderEventRouterFunction))]
    public async Task Run(
        [ServiceBusTrigger("%servicebus:topicname%", "%servicebus:subscriptionname%", Connection = "servicebus:connectionstring")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        await RouteAsync(message, messageActions, cancellationToken);
    }
}
```

### 4. Register in Program.cs

```csharp
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddServiceBusEventHandler<OrderCreatedEvent,  OrderCreatedEventHandler> ("OrderCreated");
        services.AddServiceBusEventHandler<OrderShippedEvent,  OrderShippedEventHandler> ("OrderShipped");
    })
    .Build();
```

**Routing behaviour:**
- Message with `EventType = "OrderCreated"` → `OrderCreatedEventHandler`
- Missing `EventType` → dead-lettered with reason `MissingEventType`
- Unknown `EventType` → dead-lettered with reason `UnknownEventType:{value}`
- Handler throws → message abandoned (retried by Service Bus up to `maxDeliveryCount`)

---

## Hydration

`AddServiceBusEventHandler` deserializes the message body into `TEvent`, then maps every `ApplicationProperties` entry onto a matching writable `string` property by name (case-insensitive). Domain fields declared in derived classes (e.g. `TenantId`, `OrderId`) are populated automatically — no framework changes needed.

```
Message body (JSON)           → CorrelatedEvent properties (deserialized)
message.CorrelationId         → @event.CorrelationId
ApplicationProperties["TenantId"] → @event.TenantId   (by name match)
ApplicationProperties["OrderId"]  → @event.OrderId    (by name match)
ApplicationProperties["EventType"] → skipped (routing only)
```

The property lookup map is cached per event type — reflection runs once.

---

## Publishing Queue Messages

```csharp
// Program.cs
services.AddAzureServiceBus(options =>
{
    options.ConnectionString = config["servicebus:connectionstring"];
    options.QueueName        = config["servicebus:queuename"];
});
```

```csharp
// Domain message
public class ProcessReportMessage : CorrelatedMessage
{
    public string TenantId { get; }
    public string UserId   { get; }

    public ProcessReportMessage(string jobId, string tenantId, string userId)
    {
        CorrelationId = jobId;
        TenantId      = tenantId;
        UserId        = userId;
    }

    public override Dictionary<string, string> GetApplicationProperties()
        => new() { ["TenantId"] = TenantId, ["UserId"] = UserId };
}

// Publish
await _publisher.PublishAsync(new ProcessReportMessage(jobId, tenantId, userId));
// Transport receives: CorrelationId set natively; TenantId + UserId in ApplicationProperties
```

---

## Publishing Topic Events

```csharp
await _publisher.PublishAsync(new OrderCreatedEvent
{
    CorrelationId = orderId,
    TenantId      = context.TenantId,
    UserId        = context.UserId,
    OrderId       = orderId,
});
// Transport receives: CorrelationId, EventType="OrderCreated", TenantId, UserId, OrderId in ApplicationProperties
```

---

## Single-Type Queue Handler (simple scenarios)

For queues that carry a single message type, `ServiceBusFunctionHandlerBase<TMessage>` is the simpler option:

```csharp
public class ProcessReportFunction : ServiceBusFunctionHandlerBase<ProcessReportMessage>
{
    public ProcessReportFunction(IServiceScopeFactory scopeFactory, ILogger logger)
        : base(scopeFactory, logger) { }

    [Function(nameof(ProcessReportFunction))]
    public async Task Run(
        [ServiceBusTrigger("%servicebus:queuename%", Connection = "servicebus:connectionstring")]
        ServiceBusReceivedMessage message,
        FunctionContext context)
    {
        await Handle(message, context);
    }
}
```

Implement `IMessageHandler<ProcessReportMessage>` and register it in DI.

---

## Queue Storage

```csharp
// Program.cs
services.AddAzureQueueStorage(options =>
{
    options.ConnectionString = config["queuestorage:connectionstring"];
    options.QueueName        = config["queuestorage:queuename"];
});
```

```csharp
public class BackgroundJobFunction : QueueStorageFunctionHandlerBase<BackgroundJobMessage>
{
    public BackgroundJobFunction(IStartup startup) : base(startup) { }

    protected override IStartup GetStartup() => new Startup();

    [Function(nameof(BackgroundJobFunction))]
    public async Task Run(
        [QueueTrigger("%queuestorage:queuename%", Connection = "queuestorage:connectionstring")]
        QueueMessage message,
        FunctionContext context)
    {
        await Handle(message, context);
    }
}
```

---

## Configuration Options

### `AzureServiceBusOptions`

| Property | Required | Default | Description |
|---|---|---|---|
| `ConnectionString` | Yes | — | Service Bus namespace connection string |
| `QueueName` | Yes | — | Queue or topic name for publishing |
| `MaxBatchSize` | No | `100` | Max messages per batch send |
| `TimeToLive` | No | `null` | Message TTL (null = namespace default) |
| `EnableDetailedLogging` | No | `false` | Log message IDs on each publish |

### `AzureQueueStorageOptions`

| Property | Required | Default | Description |
|---|---|---|---|
| `ConnectionString` | Yes | — | Storage account connection string |
| `QueueName` | Yes | — | Queue name |
| `VisibilityTimeout` | No | `30s` | Delay before message is visible after dequeue |
| `TimeToLive` | No | `null` | Message TTL |
| `MaxBatchSize` | No | `10` | Parallel send concurrency |
| `EnableDetailedLogging` | No | `false` | Log message IDs on each publish |

---

## Service Bus vs Queue Storage

| | Azure Service Bus | Azure Queue Storage |
|---|---|---|
| Max message size | 256 KB (std), 100 MB (premium) | 64 KB |
| Dead letter queue | Built-in | Poison queue (auto after 5 attempts) |
| Message ordering | Sessions (FIFO) | No |
| Topic routing | Yes (`EventType` property) | No |
| Native batching | Yes (up to 256 KB batch) | No (parallel send) |
| Use case | Enterprise messaging, event routing | Simple background tasks |

---

## Related Packages

| Package | Purpose |
|---------|---------|
| **AppFactory.Framework.Messaging.Core** | Platform-agnostic abstractions |
| **AppFactory.Framework.Messaging.Aws** | AWS SQS publisher + Lambda handlers |
| **AppFactory.Framework.Messaging.Azure** | This package |

---

## Resources

- [GitHub Repository](https://github.com/exiton3/AppFactory)
- [Report Issues](https://github.com/exiton3/AppFactory/issues)
