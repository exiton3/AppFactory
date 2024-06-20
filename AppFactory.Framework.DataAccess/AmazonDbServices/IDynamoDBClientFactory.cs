using Amazon.DynamoDBv2;

namespace AppFactory.Framework.DataAccess.AmazonDbServices
{
    public interface IDynamoDBClientFactory
    {
        IAmazonDynamoDB Create();
    }
}
