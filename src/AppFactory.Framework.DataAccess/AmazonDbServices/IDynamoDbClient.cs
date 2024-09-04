using Amazon.DynamoDBv2.Model;
using AppFactory.Framework.DataAccess.Configuration;

namespace AppFactory.Framework.DataAccess.AmazonDbServices;

public interface IDynamoDbClient : IDisposable
{
    Task<DynamoDbItem> GetByPrimaryKey(PrimaryKey getPrimaryKey);

    Task<bool> PutItemAsync(DynamoDbItem item);
    Task<bool> DeleteItemAsync(PrimaryKey key, CancellationToken cancellationToken = default);
    Task BatchWriteItemAsync(List<DynamoDbItem> list);
    Task<List<DynamoDbItem>> QueryAsync(QueryRequest queryRequest);
}