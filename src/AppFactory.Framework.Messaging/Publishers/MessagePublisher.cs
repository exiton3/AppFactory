using Amazon.SQS.Model;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Config;

namespace AppFactory.Framework.Messaging.Publishers;

public class MessagePublisher : IMessagePublisher
{
    private readonly ILogger _logger;
    private string SQSQueueUrl = "default";
    private IAmazonSqsClientFactory _clientFactory; 
    public MessagePublisher(ILogger logger, IConfigSettings config)
    {
        _logger = logger;
        SQSQueueUrl = config.GetValue("sqs_queue_url");
    }
    public async Task<PublishResponse> Publish<TMessage>(TMessage message, CancellationToken token = default) where TMessage : MessageBase
    {
        var sqsClient = _clientFactory.Create();
        _logger.LogInfo($"Publishing the message of type '{typeof(TMessage)}'");

        var messageRequest = CreateSendMessageRequest(SQSQueueUrl, message.Body);
        var response = await sqsClient.SendMessageAsync(messageRequest, token);
        _logger.LogTrace($"The message with MessageId:{response.MessageId}  has been pushed to SQS.");

        return new PublishResponse { MessageId = response.MessageId };
    }

    private SendMessageRequest CreateSendMessageRequest(string queueUrl, string messageBody)
    {
        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = messageBody,
        };

        return request;
    }
}