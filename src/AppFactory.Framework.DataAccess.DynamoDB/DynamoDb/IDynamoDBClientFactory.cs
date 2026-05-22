using Amazon.DynamoDBv2;

namespace AppFactory.Framework.DataAccess.DynamoDB.DynamoDb
{
    public interface IDynamoDBClientFactory
    {
        IAmazonDynamoDB Create();
        IDynamoDbClient CreateClient();
    }
}
