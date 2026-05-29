using Microsoft.Azure.Functions.Worker;
using AppFactory.Framework.EventBus.Abstractions;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace AppFactory.Framework.EventBus.Azure;

/// <summary>
/// Base class for Azure Functions that handle Event Grid events
/// Automatically deserializes Event Grid events and routes to IEventHandler
/// </summary>
public abstract class AzureFunctionEventHandlerBase<TEvent> where TEvent : IEvent
{
    protected ServiceProvider ServiceProvider;
    protected IJsonSerializer JsonSerializer;
    private IEventHandler<TEvent> _eventHandler;
    private ILogger _logger;

    protected AzureFunctionEventHandlerBase(IStartup startup = null)
    {
        InitializeServices(startup ?? GetStartup());
    }

    private void InitializeServices(IStartup startup)
    {
        var services = new ServiceCollection();
        new DependencyModule().RegisterServices(services);
        startup.ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
        JsonSerializer = ServiceProvider.GetRequiredService<IJsonSerializer>();
        _logger = ServiceProvider.GetRequiredService<ILogger>();
    }

    protected abstract IStartup GetStartup();

    /// <summary>
    /// Azure Function handler for Event Grid events
    /// </summary>
    [Function("EventHandler")]
    public async Task Run(
        [EventGridTrigger] string eventGridEvent,
        FunctionContext context)
    {
        _logger.AddTraceId(context.InvocationId);
        _logger.LogInfo($"Processing Event Grid event");

        try
        {
            var @event = JsonSerializer.Deserialize<TEvent>(eventGridEvent);

            using var scope = ServiceProvider.CreateScope();
            _eventHandler = scope.ServiceProvider.GetRequiredService<IEventHandler<TEvent>>();

            using (_logger.LogPerformance($"EventHandler: {@event.EventType}"))
            {
                await _eventHandler.HandleAsync(@event, context.CancellationToken);
            }

            _logger.LogInfo($"Event processed successfully: {@event.EventType}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing event");
            throw;
        }
    }
}
