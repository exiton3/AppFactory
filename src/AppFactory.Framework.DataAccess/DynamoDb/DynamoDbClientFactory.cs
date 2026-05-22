using Amazon.DynamoDBv2;
using AppFactory.Framework.DataAccess.Settings;
using AppFactory.Framework.Logging;

namespace AppFactory.Framework.DataAccess.DynamoDb
{
    public class DynamoDbClientFactory(IAWSSettings settings, ILogger logger) : IDynamoDBClientFactory
    {
        public IAmazonDynamoDB Create()
        {
            var client = new AmazonDynamoDBClient(settings.GetAWSRegion());

            logger.LogTrace($"Db Client #{client.GetHashCode()} created");

            return client;
        }

        public IDynamoDbClient CreateClient()
        {
            var client = new DynamoDbClient(settings, logger);

            logger.LogTrace($"Db Client #{client.GetHashCode()} created");

            return client;
        }
    }
}
