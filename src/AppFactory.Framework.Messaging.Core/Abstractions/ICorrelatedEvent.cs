namespace AppFactory.Framework.Messaging.Abstractions;

/// <summary>
/// Marker for events that carry platform-level correlation fields.
/// CorrelationId maps to the transport's native correlation identifier (e.g. Service Bus CorrelationId).
/// EventType maps to ApplicationProperties["EventType"] and drives topic subscription routing.
/// Domain-specific envelope fields (e.g. TenantId, UserId) live in derived classes.
/// </summary>
public interface ICorrelatedEvent
{
    string CorrelationId { get; }
    string EventType { get; }
}
