# AppFactory.Framework.Messaging.Core

**Platform-agnostic messaging abstractions for building multi-cloud reactive microservices.**

[![NuGet](https://img.shields.io/nuget/v/AppFactory.Framework.Messaging.Core.svg)](https://www.nuget.org/packages/AppFactory.Framework.Messaging.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## Overview

`AppFactory.Framework.Messaging.Core` provides **platform-agnostic abstractions** for queue and topic messaging in distributed systems. Write once, deploy to **AWS SQS**, **Azure Service Bus**, **Azure Queue Storage**, or any other transport without changing business logic.

### What's New in 10.7.0

- **`CorrelatedEvent`** — base class for topic events (one publisher, multiple subscribers). Domain-specific envelope fields (`TenantId`, `UserId`, etc.) live in derived classes, not in the framework.
- **`CorrelatedMessage`** — base class for queue messages (directed at a single consumer). Mirrors `CorrelatedEvent` but carries no `EventType` — the queue itself identifies the intent.
- **`ICorrelatedEnvelope`** — single interface implemented by both `CorrelatedEvent` and `CorrelatedMessage`, giving publishers one code path with no type-dispatch chain.
- **`ICorrelatedEvent`** — cleaned to framework-only fields (`CorrelationId`, `EventType`). Domain fields removed.

---

## Installation

```bash
dotnet add package AppFactory.Framework.Messaging.Core --version 10.7.0
```

**Cloud-specific implementations:**
```bash
dotnet add package AppFactory.Framework.Messaging.Azure --version 10.7.0
dotnet add package AppFactory.Framework.Messaging.Aws   --version 10.7.0
```

---

## Core Abstractions

### Topic Events — `CorrelatedEvent`

Use for events published to a topic (past tense, broadcast, multiple subscribers). Override `GetApplicationProperties()` to declare domain-specific envelope fields — the framework writes them to transport metadata on publish and hydrates them back by name on consume.

```csharp
// Framework base — only universal fields
public abstract class CorrelatedEvent : ICorrelatedEvent, ICorrelatedEnvelope
{
    public string CorrelationId { get; set; }
    public abstract string EventType { get; }
    public virtual Dictionary<string, string> GetApplicationProperties() => new();
}
```

**Domain base class (your project):**
```csharp
// Declare tenant-scoped envelope fields once — all tenant events inherit
public abstract class TenantEvent : CorrelatedEvent
{
    public string TenantId { get; set; } = default!;
    public string UserId   { get; set; } = default!;

    public override Dictionary<string, string> GetApplicationProperties()
        => new() { ["TenantId"] = TenantId, ["UserId"] = UserId };
}
```

**Concrete event:**
```csharp
public class OrderCreatedEvent : TenantEvent
{
    public override string EventType => "OrderCreated";
    public string OrderId { get; set; } = default!;
}
```

**Publish:**
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

### Queue Messages — `CorrelatedMessage`

Use for messages sent to a queue (directed intent, single consumer). No `EventType` — the queue name identifies the intent.

```csharp
public abstract class CorrelatedMessage : ICorrelatedEnvelope
{
    public string CorrelationId { get; set; }
    public virtual Dictionary<string, string> GetApplicationProperties() => new();
}
```

**Example:**
```csharp
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
```

---

### `ICorrelatedEnvelope`

Single interface for publishers — eliminates the `if (is CorrelatedEvent) else if (is IMessage)` type-dispatch chain:

```csharp
public interface ICorrelatedEnvelope
{
    string? CorrelationId { get; }
    IReadOnlyDictionary<string, string> GetEnvelopeProperties();
}
```

Both `CorrelatedEvent` and `CorrelatedMessage` implement this. The cloud-specific publishers have one check:

```csharp
if (message is ICorrelatedEnvelope envelope)
{
    if (envelope.CorrelationId is not null)
        transportMessage.CorrelationId = envelope.CorrelationId;
    foreach (var (key, value) in envelope.GetEnvelopeProperties())
        transportMessage.ApplicationProperties[key] = value;
}
```

---

### `IMessage` / `Message` — Received Messages

`IMessage` represents a message received from a transport. Use it when you need the raw message envelope (body, metadata, delivery count). `Message` implements `ICorrelatedEnvelope` for backward compatibility.

```csharp
public interface IMessage
{
    string MessageId { get; }
    string Body { get; set; }
    IDictionary<string, string> Properties { get; }
    DateTime EnqueuedTimeUtc { get; }
    int DeliveryCount { get; }
}
```

> Prefer `CorrelatedEvent` / `CorrelatedMessage` for new publish-side code. `IMessage` / `Message` are the inbound abstraction used by `ServiceBusFunctionHandlerBase` and `QueueStorageFunctionHandlerBase`.

---

### `IMessagePublisher`

```csharp
public interface IMessagePublisher
{
    Task<PublishResult> PublishAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken = default) where TMessage : class;

    Task<BatchPublishResult> PublishBatchAsync<TMessage>(
        IEnumerable<TMessage> messages,
        CancellationToken cancellationToken = default) where TMessage : class;
}
```

---

### `IMessageHandler<TMessage>` / `IMessageHandler<TMessage, TContext>`

Simple handler for fire-and-forget scenarios:

```csharp
public interface IMessageHandler<TMessage> where TMessage : class
{
    Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);
}
```

Context-based handler with explicit settlement (used by AWS Lambda where the runtime does not auto-complete):

```csharp
public interface IMessageHandler<TMessage, TContext>
    where TMessage : class
    where TContext  : IMessageContext
{
    Task HandleAsync(TMessage message, TContext context, CancellationToken cancellationToken = default);
}

public interface IMessageContext
{
    Task CompleteAsync(CancellationToken cancellationToken = default);
    Task AbandonAsync(CancellationToken cancellationToken = default);
    Task DeadLetterAsync(string reason, CancellationToken cancellationToken = default);
}
```

---

## Handler Registration

```csharp
// Manual — single handler
services.AddMessageHandler<SendConfirmationHandler, OrderCreatedMessage>();

// Assembly scan
services.AddMessageHandlers(typeof(Program).Assembly);
```

---

## Messaging vs Events

| | **Queue message** (`CorrelatedMessage`) | **Topic event** (`CorrelatedEvent`) |
|---|---|---|
| Delivery | Single consumer | Multiple subscribers |
| Routing | Queue name | `EventType` application property |
| Use case | Commands, background jobs | Domain events, integration |
| Azure | Service Bus queue | Service Bus topic/subscription |
| AWS | SQS | SNS / EventBridge |

---

## Architecture

```
┌─────────────────────────────────────────┐
│   Domain / Application Layer            │
│   - CorrelatedEvent subclasses          │
│   - CorrelatedMessage subclasses        │
│   - IMessagePublisher                   │
│   - IMessageHandler<T>                  │
└─────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│   Messaging.Core (this package)         │
│   - ICorrelatedEnvelope                 │
│   - CorrelatedEvent / CorrelatedMessage │
│   - IMessagePublisher / IMessageHandler │
│   - IMessage / Message                  │
└─────────────────────────────────────────┘
                  │
        ┌─────────┴──────────┐
        ▼                    ▼
┌──────────────────┐  ┌──────────────────┐
│  Messaging.Aws   │  │  Messaging.Azure  │
│  SqsPublisher    │  │  ServiceBus       │
│  Lambda handlers │  │  Functions        │
└──────────────────┘  └──────────────────┘
```

---

## Related Packages

| Package | Purpose |
|---------|---------|
| **AppFactory.Framework.Messaging.Core** | Platform-agnostic abstractions (this package) |
| **AppFactory.Framework.Messaging.Aws** | AWS SQS publisher + Lambda handlers |
| **AppFactory.Framework.Messaging.Azure** | Azure Service Bus + Queue Storage + Functions |

---

## Resources

- [GitHub Repository](https://github.com/exiton3/AppFactory)
- [Report Issues](https://github.com/exiton3/AppFactory/issues)
