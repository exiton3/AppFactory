using AppFactory.Framework.Messaging.Abstractions;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;

namespace AppFactory.Framework.Messaging.Azure.FunctionHandlers;

public static class ServiceBusEventHandlerExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    // Cache (Type → name-keyed property map) so reflection runs once per event type
    // and each ApplicationProperties lookup is O(1) instead of O(n).
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>>
        PropertyCache = new();

    /// <summary>
    /// Registers an event handler for a specific EventType value.
    /// The router deserializes the message body into TEvent and hydrates CorrelationId plus
    /// any ApplicationProperties entry that matches a writable property on TEvent by name.
    /// Domain-specific envelope fields (TenantId, UserId, etc.) are populated automatically
    /// without any framework changes — they just need to be settable properties on the event.
    /// </summary>
    public static IServiceCollection AddServiceBusEventHandler<TEvent, THandler>(
        this IServiceCollection services,
        string eventType)
        where TEvent : CorrelatedEvent, new()
        where THandler : class, IServiceBusEventHandler<TEvent>
    {
        services.AddScoped<IServiceBusEventHandler<TEvent>, THandler>();

        services.AddSingleton(new EventHandlerRegistration(
            eventType,
            async (message, sp, ct) =>
            {
                var @event = Hydrate<TEvent>(message);
                var handler = sp.GetRequiredService<IServiceBusEventHandler<TEvent>>();
                await handler.HandleAsync(@event, ct);
            }
        ));

        return services;
    }

    private static TEvent Hydrate<TEvent>(ServiceBusReceivedMessage message)
        where TEvent : CorrelatedEvent, new()
    {
        TEvent @event;
        try
        {
            @event = message.Body.ToObjectFromJson<TEvent>(JsonOptions)
                ?? throw new InvalidOperationException(
                       $"Message body deserialised to null for event type {typeof(TEvent).Name}.");
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to deserialise message {message.MessageId} body as {typeof(TEvent).Name}.", ex);
        }

        @event.CorrelationId = message.CorrelationId ?? string.Empty;

        if (message.ApplicationProperties.Count > 0)
        {
            var propMap = PropertyCache.GetOrAdd(typeof(TEvent), t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                 .Where(p => p.CanWrite && p.PropertyType == typeof(string))
                 .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase));

            foreach (var (key, value) in message.ApplicationProperties)
            {
                if (key == "EventType") continue;
                if (propMap.TryGetValue(key, out var prop))
                    prop.SetValue(@event, value?.ToString() ?? string.Empty);
            }
        }

        return @event;
    }
}
