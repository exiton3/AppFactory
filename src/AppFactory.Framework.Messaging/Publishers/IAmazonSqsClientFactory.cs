using Amazon.SQS;

namespace AppFactory.Framework.Messaging.Publishers;

public interface IAmazonSqsClientFactory
{

    IAmazonSQS Create();
}