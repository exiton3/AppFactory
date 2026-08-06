## AppFactory Framework 10.7.0

### Correlated Messaging — Topic Event Routing & Envelope Abstraction

---

### What's new

#### Event routing for Azure Service Bus topics

A new `ServiceBusEventRouterBase` eliminates per-event-type function boilerplate. One Azure Function handles an entire subscription; events are dispatched by `ApplicationProperties["EventType"]` to strongly-typed handlers registered in DI.

```csharp
// Program.cs
services.AddServiceBusEventHandler<OrderCreatedEvent,  OrderCreatedEventHandler>("OrderCreated");
services.AddServiceBusEventHandler<OrderShippedEvent,  OrderShippedEventHandler>("OrderShipped");
```

Messages with a missing or unregistered `EventType` are dead-lettered automatically with a descriptive reason. Handler exceptions trigger Abandon (retry). No settlement code in handler implementations.

#### Clean envelope abstraction

`ICorrelatedEnvelope` gives publishers a single code path — no more `if (is CorrelatedEvent) else if (is IMessage)` chains. Both Azure Service Bus and AWS SQS publishers are updated.

#### Domain-owned envelope fields

`TenantId`, `UserId`, and similar fields are removed from the framework base classes. Declare them once in a domain base class by overriding `GetApplicationProperties()` — the framework writes them to transport metadata on publish and hydrates them back by name on consume.

```csharp
public abstract class TenantEvent : CorrelatedEvent
{
    public string TenantId { get; set; } = default!;
    public string UserId   { get; set; } = default!;

    public override Dictionary<string, string> GetApplicationProperties()
        => new() { ["TenantId"] = TenantId, ["UserId"] = UserId };
}
```

#### Simpler CosmosDB registration

`AddCosmosRepository<TModel, TConfig>()` replaces the two-call `RegisterCosmosDbPersistence()` + `RegisterModelConfig<>()` pattern. Persistence infrastructure is registered once regardless of how many aggregate types are added.

```csharp
// Before
services.RegisterCosmosDbPersistence();
services.RegisterModelConfig<ReportJobModelConfig, ReportJob>();

// After
services.AddCosmosRepository<ReportJob, ReportJobModelConfig>();
```

---

### Packages updated

| Package | Version |
|---|---|
| `AppFactory.Framework.Messaging.Core` | 10.7.0 |
| `AppFactory.Framework.Messaging.Azure` | 10.7.0 |
| `AppFactory.Framework.DataAccess.CosmosDB` | 10.7.0 |

---

### Bug fixes

- `QueueStorageMessagePublisher` — envelope fields from `CorrelatedEvent`/`CorrelatedMessage` were silently dropped; fixed to use `ICorrelatedEnvelope` (matches the Service Bus publisher fix from 10.5.0)
- `ServiceBusEventRouterBase` — `DeadLetterMessageAsync` compiled to the wrong overload; fixed with named parameter
- Hydration — bad message body now throws `InvalidOperationException` (was swallowed silently); property lookup upgraded from O(n) scan to O(1) dictionary
