using Amazon.DynamoDBv2.Model;
using AppFactory.Framework.DataAccess.Configuration;
using AppFactory.Framework.DataAccess.Queries.Expressions;

namespace AppFactory.Framework.DataAccess.Queries;

public class QueryRequestBuilder
{
    private readonly string _tableName;
    private Dictionary<string, AttributeValue> attributeValues = new();

    private const string PkName = "pkValue";
    private const string SkName = "skValue";
    private const int DefaultLimit = 50;

    private IQueryExpression _queryExpression = new DefaultQueryExpression();

    private QueryRequestBuilder(string tableName)
    {
            
        _tableName = tableName;
    }

    public QueryRequestBuilder And => this;
    public QueryRequestBuilder Where => this;

    public static QueryRequestBuilder From(string tableName)
    {
        return new QueryRequestBuilder(tableName);
    }
        
    public QueryRequest Build()
    {

        return new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = _queryExpression.Evaluate(),
            ExpressionAttributeValues = attributeValues,
            Limit = DefaultLimit
        };
    }

    public QueryRequestBuilder SK(Action<ISKConditionOptions> keyCondition)
    {
        var skCondition = new KeyConditionOptions(DynamoDBConstants.SK, SkName);

        keyCondition(skCondition);

        attributeValues.Add($@":{SkName}", new AttributeValue { S = skCondition.AttributeValue });

        _queryExpression = _queryExpression.And(skCondition.QueryExpression);

        return this;
    }

    public QueryRequestBuilder PK(string value)
    {
        attributeValues.Add($@":{PkName}", new AttributeValue { S = value });
       _queryExpression = _queryExpression.PK();

        return this;
    }
}