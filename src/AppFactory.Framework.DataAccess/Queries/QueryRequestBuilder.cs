using Amazon.DynamoDBv2.Model;
using AppFactory.Framework.DataAccess.Configuration;
using AppFactory.Framework.DataAccess.Queries.Expressions;
using System.Linq.Expressions;
using System.Xml.Linq;
using AppFactory.Framework.Shared;

namespace AppFactory.Framework.DataAccess.Queries;

public class QueryRequestBuilder
{
    private readonly string _tableName;
    private Dictionary<string, AttributeValue> attributeValues = new();

    private const string PkName = "pkValue";
    private const string SkName = "skValue";
    private const int DefaultLimit = 50;

    private IQueryExpression _queryExpression = new DefaultQueryExpression();
    private string _gsiName;

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
            IndexName = _gsiName,
            KeyConditionExpression = _queryExpression.Evaluate(),
            ExpressionAttributeValues = attributeValues,
            Limit = DefaultLimit
        };
    }

    public QueryRequestBuilder SK(Action<ISKConditionOptions> keyCondition)
    {
        var skCondition = new KeyConditionOptions(DynamoDbConstants.SK, SkName);

        keyCondition(skCondition);

        attributeValues.Add($":{SkName}", new AttributeValue { S = skCondition.AttributeValue });

        _queryExpression = _queryExpression.And(skCondition.QueryExpression);

        return this;
    }

    public QueryRequestBuilder PK(string value)
    {
        attributeValues.Add($":{PkName}", new AttributeValue { S = value });
       _queryExpression = _queryExpression.PK();

        return this;
    }

    public QueryRequestBuilder GlobalIndex(string indexName)
    {
        _gsiName = indexName;

        return this;
    }

    public QueryRequestBuilder QueryBy(string name, string value)
    {
        attributeValues.Add($":{name}Value", new AttributeValue { S = value });
        _queryExpression = _queryExpression.Index(name);

        return this;
    }

    public QueryRequestBuilder QueryBy<TModel>(Expression<Func<TModel, object>> propertyExpression, string value)
    {
        var propName = PropertyExpressionHelper.GetPropertyName(propertyExpression).ToCamelCase();

        var propKeyValueName = propName + "Value";


        attributeValues.Add($":{propKeyValueName}", new AttributeValue { S = value });

        _queryExpression = _queryExpression.Index(propName);

        return this;
    }

    public QueryRequestBuilder ThenBy(string name, Action<ISKConditionOptions> keyCondition)
    {

        var skCondition = new KeyConditionOptions(name, $"{name}Value");

        keyCondition(skCondition);

        attributeValues.Add($":{name}Value", new AttributeValue { S = skCondition.AttributeValue });

        _queryExpression = _queryExpression.And(skCondition.QueryExpression);

        return this;
    }

    public QueryRequestBuilder ThenBy<TModel>(Expression<Func<TModel, object>> propertyExpression, Action<ISKConditionOptions> keyCondition)
    {
        var propName = PropertyExpressionHelper.GetPropertyName(propertyExpression).ToCamelCase();

        var propKeyValueName = propName + "Value";


        var skCondition = new KeyConditionOptions(propName, propKeyValueName);

        keyCondition(skCondition);

        attributeValues.Add($":{propKeyValueName}", new AttributeValue { S = skCondition.AttributeValue });

        _queryExpression = _queryExpression.And(skCondition.QueryExpression);

        return this;
    }
}

public static class StringExtensions
{
    public static string ToCamelCase(this string value)
    {
       return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }
}