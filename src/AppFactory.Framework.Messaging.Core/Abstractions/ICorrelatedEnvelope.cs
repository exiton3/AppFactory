namespace AppFactory.Framework.Messaging.Abstractions;

/// <summary>
/// Implemented by CorrelatedEvent (topic) and CorrelatedMessage (queue) so publishers
/// have a single path with no type-dispatch chain.
/// CorrelationId maps to the transport's native correlation field (e.g. Service Bus CorrelationId).
/// GetEnvelopeProperties returns key/value pairs written to the transport metadata
/// (ApplicationProperties / MessageAttributes).
/// </summary>
public interface ICorrelatedEnvelope
{
    string? CorrelationId { get; }
    IReadOnlyDictionary<string, string> GetEnvelopeProperties();
}
