using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Messaging.LambdaHandlers;

/// <summary>
/// Base class for Lambda handler to handle messages from SQS queue
/// </summary>
/// <typeparam name="TMessage">message</typeparam>
public abstract class LambdaMessageHandlerBase<TMessage> where TMessage : Message, new()
{
    protected ServiceProvider ServiceProvider;
    protected IJsonSerializer JsonSerializer;
    private ILambdaMessageProcessor<TMessage> _processor;
    private ILogger _logger;
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
        _logger = ServiceProvider.GetRequiredService<ILogger>();
        _logger.LogInfo($"New instance of Lambda EventHandler {GetHashCode()} created");
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
        _logger.AddTraceId(context.AwsRequestId);

        try
        {
            foreach (var message in @event.Records)
            {
                await ProcessMessageAsync(message, context);
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e, $"{e.Message} -- {e.StackTrace}");

            throw;
        }
    }
    private async Task ProcessMessageAsync(SQSEvent.SQSMessage sqsMessage, ILambdaContext context)
    {
        try
        {
            context.Logger.LogDebug($"Processed message {sqsMessage.Body}");

            _logger.LogInfo($"Message {sqsMessage.MessageId} from {sqsMessage.EventSource} received");
            _logger.LogTrace(
                $"Message {sqsMessage.MessageId} from {sqsMessage.EventSource} received {sqsMessage.Body} Attributes ,{sqsMessage.MessageAttributes.Keys.Count}");
            
            using var scope = ServiceProvider.CreateScope();
            _processor = scope.ServiceProvider.GetRequiredService<ILambdaMessageProcessor<TMessage>>();

            _logger.LogTrace($"SQS Message Processor #{_processor.GetHashCode()} {_processor.GetType().Name} started");
            using (_logger.LogPerformance($"Processor #{_processor.GetHashCode()} {_processor.GetType().Name}"))
            {
                var message = MapMessage(sqsMessage);

                await _processor.Process(message);
            }
        }
        catch (Exception e)
        {
            //TODO: use Dead Letter Queue to handle failures. By configuring a Lambda DLQ.
            context.Logger.LogError($"An error occurred {e.StackTrace}");
            _logger.LogError(e, "Unhandled exception");
            throw;
        }

    }

    private TMessage MapMessage(SQSEvent.SQSMessage sqsMessage)
    {
        var message = new TMessage
        {
            Body = sqsMessage.Body,
            MessageId = sqsMessage.MessageId,
            Source = sqsMessage.EventSource,
            Attributes = sqsMessage.MessageAttributes.ToDictionary(x=>x.Key, v=>v.Value.StringValue)
        };

        return message;
    }
}