using Amazon.SQS;

namespace AppFactory.Framework.Messaging.Publishers;

internal class AmazonSqsClientFactory : IAmazonSqsClientFactory
{
    public IAmazonSQS Create()
    {
        return new AmazonSQSClient();
    }
}