namespace AppFactory.Framework.Messaging.Abstractions;

/// <summary>
/// Base class for messages published to a queue — directed at a single consumer, no routing key.
/// Pairs with CorrelatedEvent (topic / routed broadcast); the difference is that queue messages
/// carry no EventType because the queue itself identifies the intent.
///
/// Override GetApplicationProperties() in domain classes to declare envelope fields
/// (e.g. TenantId, UserId). The framework writes them to the transport metadata on publish
/// and hydrates them back onto matching properties by name on consume.
/// </summary>
public abstract class CorrelatedMessage : ICorrelatedEnvelope
{
    public string CorrelationId { get; set; } = default!;

    public virtual Dictionary<string, string> GetApplicationProperties() => new();

    string? ICorrelatedEnvelope.CorrelationId => CorrelationId;
    IReadOnlyDictionary<string, string> ICorrelatedEnvelope.GetEnvelopeProperties()
        => GetApplicationProperties();
}
