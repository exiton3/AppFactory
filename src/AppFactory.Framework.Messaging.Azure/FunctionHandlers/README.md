# FunctionHandlers

Azure Functions handler base classes for Service Bus and Queue Storage.

---

## ServiceBusEventRouterBase — multi-event topic routing (recommended)

Use when one subscription receives multiple event types distinguished by `ApplicationProperties["EventType"]`. This is the preferred pattern for topic consumers in the isolated worker model.

**How it works:**
1. Reads `ApplicationProperties["EventType"]` from the incoming message.
2. Looks up the registered `EventHandlerRegistration` for that type.
3. Deserializes the message body and hydrates matching properties from `ApplicationProperties`.
4. Calls the handler in a DI scope.
5. Completes the message on success; Abandons on handler exception; DeadLetters on missing or unknown EventType.

**Register handlers in Program.cs:**

```csharp
services.AddServiceBusEventHandler<OrderCreatedEvent,  OrderCreatedEventHandler> ("OrderCreated");
services.AddServiceBusEventHandler<OrderShippedEvent,  OrderShippedEventHandler> ("OrderShipped");
```

**Expose the trigger method:**

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

**Implement handlers:**

```csharp
public class OrderCreatedEventHandler : IServiceBusEventHandler<OrderCreatedEvent>
{
    private readonly ICommandDispatcher _dispatcher;

    public OrderCreatedEventHandler(ICommandDispatcher dispatcher)
        => _dispatcher = dispatcher;

    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // TenantId, OrderId, etc. are hydrated from ApplicationProperties by name
        var result = await _dispatcher.Dispatch(
            new HandleOrderCreatedCommand { OrderId = @event.OrderId, TenantId = @event.TenantId },
            cancellationToken);

        if (!result.IsSuccess)
            throw new InvalidOperationException($"Handler failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        // Throw to trigger Abandon (retry). Router handles settlement — do not call Complete/Abandon here.
    }
}
```

**Hydration rules:**
- `message.CorrelationId` → `@event.CorrelationId`
- Each `ApplicationProperties` entry → writable `string` property on the event with matching name (case-insensitive)
- `ApplicationProperties["EventType"]` is skipped (routing only)
- Property lookup is cached per event type (O(1) after first message)

---

## ServiceBusFunctionHandlerBase — single-type queue handler

Use for queues that carry a single message type. Simpler than the router — no event type dispatch needed.

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

Register `IMessageHandler<ProcessReportMessage>` in DI. The base class resolves it from the host scope per message.

Batch variant:

```csharp
[Function("ProcessReportBatch")]
public async Task RunBatch(
    [ServiceBusTrigger("%servicebus:queuename%", Connection = "servicebus:connectionstring")]
    ServiceBusReceivedMessage[] messages,
    FunctionContext context)
{
    await HandleBatch(messages, context);
}
```

---

## QueueStorageFunctionHandlerBase — Queue Storage handler

Handles messages from Azure Queue Storage. Uses `IStartup` for dependency registration (legacy pattern — does not use the host DI container directly).

```csharp
public class NotificationFunction : QueueStorageFunctionHandlerBase<NotificationMessage>
{
    public NotificationFunction() : base(new Startup()) { }

    protected override IStartup GetStartup() => new Startup();

    [Function(nameof(NotificationFunction))]
    public async Task Run(
        [QueueTrigger("%queuestorage:queuename%", Connection = "queuestorage:connectionstring")]
        QueueMessage message,
        FunctionContext context)
    {
        await Handle(message, context);
    }
}
```

String body variant (skips Queue SDK deserialization):

```csharp
[Function("ProcessSimpleMessage")]
public async Task RunString(
    [QueueTrigger("%queuestorage:queuename%", Connection = "queuestorage:connectionstring")]
    string messageBody,
    FunctionContext context)
{
    await HandleString(messageBody, context);
}
```

Message class must inherit from `Message`:

```csharp
public class NotificationMessage : Message
{
    public string RecipientId { get; set; } = default!;
    public string Template    { get; set; } = default!;
}
```

---

## Choosing the right base class

| Scenario | Use |
|---|---|
| Topic with multiple event types | `ServiceBusEventRouterBase` + `IServiceBusEventHandler<TEvent>` |
| Queue with a single message type | `ServiceBusFunctionHandlerBase<TMessage>` |
| Azure Queue Storage | `QueueStorageFunctionHandlerBase<TMessage>` |
