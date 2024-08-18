using Amazon.Lambda.CloudWatchEvents;
using Amazon.Lambda.Core;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.EventBus.EventBus;
using AppFactory.Framework.EventBus.EventBus.Events;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.EventBus.Aws.LambdaHandlers;

public abstract class LambdaEventHandlerBase<TEvent> where TEvent:IntegrationEvent
{
    protected ServiceProvider ServiceProvider;
    protected IJsonSerializer JsonSerializer;
   private IIntegrationEventHandler<TEvent> _handler;
    private ILogger _log;
    private IStartup _startup;
    protected LambdaEventHandlerBase(IStartup startup = null)
    {
        _startup = startup;
        InitializeServices();
    }

    private void InitializeServices()
    {
        var services = new ServiceCollection();

        ConfigureServicesInt(services);
        ServiceProvider = services.BuildServiceProvider();
        JsonSerializer = ServiceProvider.GetRequiredService<IJsonSerializer>();
        _log = ServiceProvider.GetRequiredService<ILogger>();
        _log.LogInfo($"New instance of Lambda EventHandler {GetHashCode()} created");
    }

    private void ConfigureServicesInt(IServiceCollection services)
    {
        new DependencyModule().RegisterServices(services);

        _startup ??= GetStartup();
        _startup.ConfigureServices(services);
    }

    protected abstract IStartup GetStartup();
    public async Task Handle(CloudWatchEvent<TEvent> @event, ILambdaContext context)
    {
        _log.AddTraceId(context.AwsRequestId);

        try
        {
            var eventDetail = JsonSerializer.Serialize(@event.Detail);
            _log.LogInfo($"Event {@event.DetailType} from {@event.Source} received {eventDetail}");

            using var scope = ServiceProvider.CreateScope();
            _handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<TEvent>>();

            _log.LogTrace($"EventHandler #{_handler.GetHashCode()} {_handler.GetType().Name} started");
            using (_log.LogPerformance($"Processor #{_handler.GetHashCode()} {_handler.GetType().Name}"))
            {
                 await _handler.Handle(@event.Detail);
            }
        }
        catch (Exception e)
        {
            _log.LogError(e, $"{e.Message} -- {e.StackTrace}");
        }
    }

 
}