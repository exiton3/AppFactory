using Amazon.DynamoDBv2;

namespace AppFactory.Framework.DataAccess.DynamoDb
{
    public interface IDynamoDBClientFactory
    {
        IAmazonDynamoDB Create();
        IDynamoDbClient CreateClient();
    }
}
