using System.Text.Json;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using AppFactory.Framework.EventBus.EventBus;
using AppFactory.Framework.EventBus.EventBus.Events;
using AppFactory.Framework.EventBus.EventBus.Subscriptions;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Config;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.EventBus.Aws.EventBus;

internal class EventBridgeServiceBus : IEventBus
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventBusSubscriptionsManager _subsManager;
    private const string INTEGRATION_EVENT_SUFFIX = "IntegrationEvent";
    private string EventBusName = "default";

    private readonly IAmazonEventBridge _eventBridge;

    private readonly IAmazonEventBridgeFactory _eventBridgeFactory;
    public EventBridgeServiceBus(ILogger logger, IServiceProvider serviceProvider, IEventBusSubscriptionsManager subsManager, IAmazonEventBridgeFactory eventBridgeFactory, IConfigSettings config)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _subsManager = subsManager;
        _eventBridgeFactory = eventBridgeFactory;
        EventBusName = config.GetValue("eventbus_name");
        _eventBridge = _eventBridgeFactory.Create();
    }

    public void Publish<TEvent>(TEvent @event) where TEvent : IntegrationEvent
    {
        var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFFIX, "");

        var serializedEvent = JsonSerializer.Serialize(@event);
        var message = new PutEventsRequestEntry
        {
            Detail = serializedEvent,
            DetailType = eventName,
            EventBusName = EventBusName,
            Source = @event.Source,

        };


        _logger.LogInfo($"Event will publish to Event Bus: @{serializedEvent}");

        var putRequest = new PutEventsRequest
        {
            Entries = new List<PutEventsRequestEntry> { message }
        };

        var response = _eventBridge.PutEventsAsync(putRequest).Result;

        _logger.LogInfo($@"Event {eventName} published to event bus {response.HttpStatusCode} #{response.ResponseMetadata.RequestId}");
    }

    public void Subscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFFIX, "");

        var containsKey = _subsManager.HasSubscriptionsForEvent<T>();
        if (containsKey)
        {
            
        }
        _logger.LogInfo($"Subscribing to event {eventName} with {typeof(TH).Name}");

        _subsManager.AddSubscription<T, TH>();
    }

    public void Unsubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFFIX, "");

        try
        {
            //_serviceBusPersisterConnection
            //    .AdministrationClient
            //    .DeleteRuleAsync(_topicName, _subscriptionName, eventName)
            //    .GetAwaiter()
            //    .GetResult();
        }
        catch (Exception ex) // when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
        {
            _logger.LogInfo($"Warring: The messaging entity {eventName} Could not be found.");
        }

        _logger.LogInfo($"Unsubscribing from event {eventName}");

        _subsManager.RemoveSubscription<T, TH>();
    }

    private async Task<bool> ProcessEvent(string eventName, string message)
    {
        var processed = false;
        if (_subsManager.HasSubscriptionsForEvent(eventName))
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var subscriptions = _subsManager.GetHandlersForEvent(eventName);
            foreach (var subscription in subscriptions)
            {

                var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                if (handler == null) continue;
                var eventType = _subsManager.GetEventTypeByName(eventName);
                var integrationEvent = JsonSerializer.Deserialize(message, eventType);
                var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                await (Task)concreteType.GetMethod("Handle").Invoke(handler, new[] { integrationEvent });

            }
        }
        processed = true;

        return processed;
    }
}