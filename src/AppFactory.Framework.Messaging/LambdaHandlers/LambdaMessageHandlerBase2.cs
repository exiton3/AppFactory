using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.Extensions.DependencyInjection;
using static Amazon.Lambda.SQSEvents.SQSBatchResponse;

namespace AppFactory.Framework.Messaging.LambdaHandlers;

/// <summary>
/// Base class for Lambda handler to handle messages from SQS queue
/// </summary>
/// <typeparam name="TMessage">message</typeparam>
public abstract class LambdaMessageHandlerBase2<TMessage> where TMessage : Message, new()
{
    protected ServiceProvider ServiceProvider;
    protected IJsonSerializer JsonSerializer;
    private ILambdaMessageProcessor<TMessage> _processor;
    private ILogger _logger;
    private IStartup _startup;
    protected LambdaMessageHandlerBase2(IStartup startup = null)
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

    public async Task<SQSBatchResponse> Handle(SQSEvent @event, ILambdaContext context)
    {
        _logger.AddTraceId(context.AwsRequestId);
        var batchItemFailures = new List<BatchItemFailure>();
        _logger.LogInfo($"Records {@event.Records.Count} received");

        foreach (var message in @event.Records)
        {
            try
            {
                await ProcessMessageAsync(message, context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{e.Message} -- {e.StackTrace}");
                batchItemFailures.Add(new BatchItemFailure
                {
                    ItemIdentifier = message.MessageId
                });
            }
        }

        return new SQSBatchResponse(batchItemFailures);
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage sqsMessage, ILambdaContext context)
    {
        try
        {
            _logger.LogInfo($"Message {sqsMessage.MessageId} from {sqsMessage.EventSource} received");

            var attributes = GetLogMessageForAttributes(sqsMessage);

            _logger.LogTrace($"Message {sqsMessage.MessageId} from {sqsMessage.EventSource} received with Attributes ,{attributes}");
            
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
            context.Logger.LogError($"An error occurred {e.StackTrace}");
            _logger.LogError(e, "Unhandled exception");
            throw;
        }

    }

    private static string GetLogMessageForAttributes(SQSEvent.SQSMessage sqsMessage)
    {
        return string.Join(',', sqsMessage.MessageAttributes.Select(x => x.Key + "-" + x.Value));
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