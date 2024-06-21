using Amazon.DynamoDBv2;
using AppFactory.Framework.DataAccess.Configuration;
using AppFactory.Framework.Logging;

namespace AppFactory.Framework.DataAccess.AmazonDbServices
{
    public class DynamoDbClientFactory : IDynamoDBClientFactory
    {
        private readonly IAWSSettings _offersSettings;
        private readonly ILogger _logger;

        public DynamoDbClientFactory(IAWSSettings offersSettings, ILogger logger)
        {
            _offersSettings = offersSettings;
            _logger = logger;
        }
        public IAmazonDynamoDB Create()
        {
            var client = new AmazonDynamoDBClient(_offersSettings.GetAWSRegion());
            
            _logger.LogTrace($"Db Client #{client.GetHashCode()} created");

            return client;
        }
    }
}
