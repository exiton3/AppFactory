using Amazon.DynamoDBv2.Model;

namespace AppFactory.Framework.DataAccess.Models;

public class PrimaryKey
{
    public string PK { get; set; }
    public string SK { get; set; }

    public Dictionary<string, AttributeValue> ToAttributeValues()
    {
        return new Dictionary<string, AttributeValue>
        {
            [DynamoDbKeyConstants.PK] = new() { S = PK },
            [DynamoDbKeyConstants.SK] = new() { S = SK }
        };
    }
}