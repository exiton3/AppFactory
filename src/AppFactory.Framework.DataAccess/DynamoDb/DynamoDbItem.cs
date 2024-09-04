using Amazon.DynamoDBv2.Model;

namespace AppFactory.Framework.DataAccess.DynamoDb;

public class DynamoDbItem : Dictionary<string, AttributeValue>
{
    public DynamoDbItem(Dictionary<string, AttributeValue> items)
    {
        foreach (var item in items)
        {
            Add(item.Key, item.Value);
        }
    }
}