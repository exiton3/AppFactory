using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Messaging;

public abstract class LambdaMessageHandlerBase<TMessage> where TMessage : Message
{
    protected ServiceProvider ServiceProvider;
    protected IJsonSerializer JsonSerializer;
    private ILambdaMessageProcessor<TMessage> _handler;
    private ILogger _log;
    private IStartup _startup;
    protected LambdaMessageHandlerBase(IStartup startup = null)
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
    public async Task Handle(SQSEvent @event, ILambdaContext context)
    {
        _log.AddTraceId(context.AwsRequestId);

        try
        {
            foreach (var message in @event.Records)
            {
                await ProcessMessageAsync(message, context);
            }
               
        }
        catch (Exception e)
        {
            _log.LogError(e, $"{e.Message} -- {e.StackTrace}");
        }
    }
    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Processed message {message.Body}");

            var eventDetail = JsonSerializer.Serialize(message.Body);
            _log.LogInfo($"Message {message.MessageId} from {message.EventSource} received {eventDetail}");

            using var scope = ServiceProvider.CreateScope();
            _handler = scope.ServiceProvider.GetRequiredService<ILambdaMessageProcessor<TMessage>>();

            _log.LogTrace($" SQS EventHandler #{_handler.GetHashCode()} {_handler.GetType().Name} started");
            using (_log.LogPerformance($"Processor #{_handler.GetHashCode()} {_handler.GetType().Name}"))
            {
                var deserializedMessage = JsonSerializer.Deserialize<TMessage>(message.Body);

                await _handler.Handle(deserializedMessage);
            }
               
            await Task.CompletedTask;
        }
        catch (Exception e)
        {
            //You can use Dead Letter Queue to handle failures. By configuring a Lambda DLQ.
            context.Logger.LogError($"An error occurred");
            throw;
        }

    }

}