using Amazon.DynamoDBv2.Model;

namespace AppFactory.Framework.DataAccess.Configuration;

public class PrimaryKey
{
    public string PK { get; set; }
    public string SK { get; set; }

    public Dictionary<string, AttributeValue> ToAttributeValues()
    {
        return new Dictionary<string, AttributeValue>
        {
            [DynamoDBConstants.PK] = new() { S = PK },
            [DynamoDBConstants.SK] = new() { S = SK }
        };
    }
}