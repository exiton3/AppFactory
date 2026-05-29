using Azure;
using Azure.Messaging.EventGrid;
using AppFactory.Framework.EventBus.Abstractions;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Serialization;
using AzureCloudEvent = Azure.Messaging.CloudEvent;

namespace AppFactory.Framework.EventBus.Azure;

/// <summary>
/// Azure Event Grid publisher
/// Publishes CloudEvents to Azure Event Grid topics
/// </summary>
public class EventGridPublisher : IEventPublisher
{
    private readonly EventGridPublisherClient _client;
    private readonly IJsonSerializer _serializer;
    private readonly ILogger _logger;

    public EventGridPublisher(
        EventGridPublisherClient client,
        IJsonSerializer serializer,
        ILogger logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        _logger.LogInfo($"Publishing event {@event.EventType} to Event Grid");

        var cloudEvent = CreateCloudEvent(@event);

        using (_logger.LogPerformance($"EventGrid.SendEvent - {@event.EventType}"))
        {
            await _client.SendEventAsync(cloudEvent, cancellationToken);
            _logger.LogInfo($"Event {@event.EventType} published successfully. EventId: {@event.EventId}");
        }
    }

    public async Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var eventList = events.ToList();
        _logger.LogInfo($"Publishing {eventList.Count} events to Event Grid");

        var cloudEvents = eventList.Select(CreateCloudEvent).ToArray();

        using (_logger.LogPerformance($"EventGrid.SendEvents - Batch of {cloudEvents.Length}"))
        {
            await _client.SendEventsAsync(cloudEvents, cancellationToken);
            _logger.LogInfo($"Published {eventList.Count} events successfully");
        }
    }

    private AzureCloudEvent CreateCloudEvent<TEvent>(TEvent @event) where TEvent : IEvent
    {
        return new AzureCloudEvent(
            source: @event.Source,
            type: @event.EventType,
            jsonSerializableData: @event)
        {
            Id = @event.EventId,
            Time = new DateTimeOffset(@event.OccurredAt),
            DataContentType = "application/json",
            Subject = @event.Metadata.TryGetValue("subject", out var subject) ? subject : null
        };
    }
}
