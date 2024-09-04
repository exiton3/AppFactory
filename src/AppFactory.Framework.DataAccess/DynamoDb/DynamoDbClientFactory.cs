using Amazon.DynamoDBv2;
using AppFactory.Framework.DataAccess.Settings;
using AppFactory.Framework.Logging;

namespace AppFactory.Framework.DataAccess.DynamoDb
{
    public class DynamoDbClientFactory : IDynamoDBClientFactory
    {
        private readonly IAWSSettings _settings;
        private readonly ILogger _logger;

        public DynamoDbClientFactory(IAWSSettings settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public IAmazonDynamoDB Create()
        {
            var client = new AmazonDynamoDBClient(_settings.GetAWSRegion());

            _logger.LogTrace($"Db Client #{client.GetHashCode()} created");

            return client;
        }

        public IDynamoDbClient CreateClient()
        {
            var client = new DynamoDbClient(_settings, _logger);

            _logger.LogTrace($"Db Client #{client.GetHashCode()} created");

            return client;
        }
    }
}
