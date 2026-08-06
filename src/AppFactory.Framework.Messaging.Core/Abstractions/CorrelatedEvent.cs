namespace AppFactory.Framework.Messaging.Abstractions;

/// <summary>
/// Base class for all correlated events. Only universal transport fields live here;
/// domain-specific envelope fields (e.g. TenantId, UserId) belong in derived classes.
///
/// On publish: override GetApplicationProperties() to declare which extra fields are written
/// into the transport message's ApplicationProperties.
///
/// On consume: the router hydrates CorrelationId from the native transport field, then maps
/// each ApplicationProperties entry onto a matching settable property by name — so any
/// property declared in a derived class is populated automatically without framework changes.
/// </summary>
public abstract class CorrelatedEvent : ICorrelatedEvent, ICorrelatedEnvelope
{
    public string CorrelationId { get; set; } = default!;

    public abstract string EventType { get; }

    /// <summary>
    /// Returns the key/value pairs that will be written to ApplicationProperties on the outbound
    /// transport message. Override in domain base classes to add envelope fields (e.g. TenantId).
    /// </summary>
    public virtual Dictionary<string, string> GetApplicationProperties()
        => new();

    // ICorrelatedEnvelope — explicit so the class surface stays clean.
    // EventType is folded in here so publishers never need to know about it separately.
    string? ICorrelatedEnvelope.CorrelationId => CorrelationId;
    IReadOnlyDictionary<string, string> ICorrelatedEnvelope.GetEnvelopeProperties()
    {
        var props = GetApplicationProperties();
        props["EventType"] = EventType;
        return props;
    }
}
