using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Messaging.Abstractions;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.Extensions.DependencyInjection;
using static Amazon.Lambda.SQSEvents.SQSBatchResponse;
using CoreMessage = AppFactory.Framework.Messaging.Abstractions.Message;

namespace AppFactory.Framework.Messaging.Aws.Handlers;

/// <summary>
/// AWS Lambda SQS message handler base class with DI support.
/// Implements Publisher-Subscriber pattern for AWS SQS integration.
/// Supports IMessageHandler (cloud-agnostic) pattern.
/// </summary>
/// <typeparam name="TMessage">The message type to handle (must inherit from Message class)</typeparam>
public abstract class SqsMessageHandlerBase<TMessage> where TMessage : CoreMessage, new()
{
    protected ServiceProvider ServiceProvider;
    protected IJsonSerializer JsonSerializer;
    private IMessageHandler<TMessage> _handler;
    private ILogger _logger;
    private IStartup _startup;

    protected SqsMessageHandlerBase(IStartup startup = null)
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
        _logger.LogInfo($"New instance of SQS Message Handler {GetHashCode()} created");
    }

    private void ConfigureServicesInt(IServiceCollection services)
    {
        _startup ??= GetStartup();
        _startup.ConfigureServices(services);
    }

    protected abstract IStartup GetStartup();

    /// <summary>
    /// Lambda entry point - processes SQS event batch
    /// </summary>
    public async Task<SQSBatchResponse> Handle(SQSEvent @event, ILambdaContext context)
    {
        _logger.AddTraceId(context.AwsRequestId);
        var batchItemFailures = new List<BatchItemFailure>();
        _logger.LogInfo($"SQS batch of {@event.Records.Count} messages received");

        foreach (var sqsMessage in @event.Records)
        {
            try
            {
                await ProcessMessageAsync(sqsMessage, context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to process message {sqsMessage.MessageId}: {e.Message}");
                batchItemFailures.Add(new BatchItemFailure
                {
                    ItemIdentifier = sqsMessage.MessageId
                });
            }
        }

        if (batchItemFailures.Any())
        {
            _logger.LogInfo($"{batchItemFailures.Count} messages failed and will be retried");
        }

        return new SQSBatchResponse(batchItemFailures);
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage sqsMessage, ILambdaContext context)
    {
        try
        {
            _logger.LogInfo($"Processing SQS message {sqsMessage.MessageId} from {sqsMessage.EventSource}");

            var attributes = GetLogMessageForAttributes(sqsMessage);
            _logger.LogTrace($"Message {sqsMessage.MessageId} attributes: {attributes}");

            using var scope = ServiceProvider.CreateScope();
            _handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();

            _logger.LogTrace($"Message Handler #{_handler.GetHashCode()} {_handler.GetType().Name} started");
            
            using (_logger.LogPerformance($"Handler #{_handler.GetHashCode()} {_handler.GetType().Name}"))
            {
                var message = MapSqsMessageToMessage(sqsMessage);
                await _handler.HandleAsync(message, context.RemainingTime.ToCancellationToken());
            }

            _logger.LogInfo($"Successfully processed message {sqsMessage.MessageId}");
        }
        catch (Exception e)
        {
            context.Logger.LogError($"Error processing message {sqsMessage.MessageId}: {e.StackTrace}");
            _logger.LogError(e, "Unhandled exception in message handler");
            throw;
        }
    }

    private static string GetLogMessageForAttributes(SQSEvent.SQSMessage sqsMessage)
    {
        var attributes = sqsMessage.MessageAttributes
            .Select(x => $"{x.Key}={x.Value.StringValue}")
            .ToList();

        attributes.Add($"ApproximateReceiveCount={sqsMessage.Attributes.GetValueOrDefault("ApproximateReceiveCount", "0")}");
        attributes.Add($"SentTimestamp={sqsMessage.Attributes.GetValueOrDefault("SentTimestamp", "0")}");

        return string.Join(", ", attributes);
    }

    /// <summary>
    /// Maps AWS SQS message to platform-agnostic IMessage
    /// </summary>
    private TMessage MapSqsMessageToMessage(SQSEvent.SQSMessage sqsMessage)
    {
        var message = new TMessage
        {
            MessageId = sqsMessage.MessageId,
            Body = sqsMessage.Body,
            Properties = MapMessageAttributes(sqsMessage),
            EnqueuedTimeUtc = ParseSentTimestamp(sqsMessage),
            DeliveryCount = ParseDeliveryCount(sqsMessage)
        };

        return message;
    }

    private static Dictionary<string, string> MapMessageAttributes(SQSEvent.SQSMessage sqsMessage)
    {
        var properties = new Dictionary<string, string>();

        // Map custom message attributes
        foreach (var attr in sqsMessage.MessageAttributes)
        {
            properties[attr.Key] = attr.Value.StringValue;
        }

        // Map system attributes
        properties["EventSource"] = sqsMessage.EventSource;
        properties["EventSourceARN"] = sqsMessage.EventSourceArn;
        properties["AWSRegion"] = sqsMessage.AwsRegion;

        foreach (var attr in sqsMessage.Attributes)
        {
            properties[$"SQS_{attr.Key}"] = attr.Value;
        }

        return properties;
    }

    private static DateTime ParseSentTimestamp(SQSEvent.SQSMessage sqsMessage)
    {
        if (sqsMessage.Attributes.TryGetValue("SentTimestamp", out var timestamp) 
            && long.TryParse(timestamp, out var milliseconds))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
        }

        return DateTime.UtcNow;
    }

    private static int ParseDeliveryCount(SQSEvent.SQSMessage sqsMessage)
    {
        if (sqsMessage.Attributes.TryGetValue("ApproximateReceiveCount", out var count) 
            && int.TryParse(count, out var deliveryCount))
        {
            return deliveryCount;
        }

        return 1;
    }
}

/// <summary>
/// Extension methods for Lambda context
/// </summary>
internal static class LambdaContextExtensions
{
    public static CancellationToken ToCancellationToken(this TimeSpan remainingTime)
    {
        if (remainingTime <= TimeSpan.Zero)
        {
            return CancellationToken.None;
        }

        var cts = new CancellationTokenSource(remainingTime);
        return cts.Token;
    }
}
