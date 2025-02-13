using AppFactory.Framework.DataAccess.Queries;
using AppFactory.Framework.TestExtensions;
using Xunit;

namespace AppFactory.Framework.DataAccess.UnitTests;

public class QueryRequestBuilderTests
{

    [Fact]
    public void PKIsSet_SetsPKEqualsConditionExpression()
    {
        var request = QueryRequestBuilder.From("TableName")
            .Where.PK("pk_value")
            .Build();

        request.KeyConditionExpression.ShouldBeEqualTo("PK = :pkValue");
    }

    [Fact]
    public void PK_And_SK_SetsKeyConditionExpression()
    {
        var request = QueryRequestBuilder.From("TableName")
            .Where.PK("pk_value").And.SK(x=>x.BeginsWith("some Value"))
            .Build();

        request.KeyConditionExpression.ShouldBeEqualTo("PK = :pkValue and begins_with(SK,:skValue)");

    }

    [Fact]
    public void For_SetsTableName()
    {
        var request = QueryRequestBuilder.From("TableName")
            .PK("pk_value")
            .Build();

        request.TableName.ShouldBeEqualTo("TableName");
    }


    [Fact]
    public void PK_SetsPkAttributeValue()
    {
        var request = QueryRequestBuilder.From("TableName")
            .PK("pk_value")
            .Build();

        request.ExpressionAttributeValues[":pkValue"].S.ShouldBeEqualTo("pk_value");
    }

    [Fact]
    public void SK_Sets_SK_AttributeValue()
    {
        var request = QueryRequestBuilder.From("TableName")
            .PK("pk_value")
            .SK(x=>x.Equals("sk_value"))
            .Build();

        request.ExpressionAttributeValues[":skValue"].S.ShouldBeEqualTo("sk_value");
    }

    [Fact]
    public void SK_EqualsTo_AddEqualsCondition()
    {
        var request = QueryRequestBuilder.From("TableName")
            .PK("pk_value")
            .SK(x => x.Equals("sk_value"))
            .Build();

        request.KeyConditionExpression.ShouldBeEqualTo("PK = :pkValue and SK = :skValue");
    }

    [Fact]
    public void SK_BeginsWith_AddCondition()
    {
        var request = QueryRequestBuilder.From("TableName")
            .PK("pk_value")
            .SK(x => x.BeginsWith("sk_value"))
            .Build();

        request.KeyConditionExpression.ShouldBeEqualTo("PK = :pkValue and begins_with(SK,:skValue)");
    }

    [Fact]
    public void Gsi_SupportedInQueryRequest()
    {
        var request = QueryRequestBuilder.From("TableName")
            .GlobalIndex("indexName")
            .Build();

        request.IndexName.ShouldBeEqualTo("indexName");
    }


    [Fact]
    public void QueryBy_Custom_KeyConditionExpression()
    {
        var request = QueryRequestBuilder.From("TableName")
            .GlobalIndex("indexName")
            .QueryBy<SampleModel>(x => x.Name, "someValue")
            .Build();
        request.ExpressionAttributeValues[":nameValue"].S.ShouldBeEqualTo("someValue");
        request.KeyConditionExpression.ShouldBeEqualTo("name = :nameValue");
    }

    [Fact]
    public void ThenBy_KeyConditionExpression()
    {
        var request = QueryRequestBuilder.From("TableName")
            .GlobalIndex("indexName")
            .QueryBy("fieldName", "someValue")
            .ThenBy("name", x=>x.Equals("secondValue"))
            .Build();
        request.ExpressionAttributeValues[":nameValue"].S.ShouldBeEqualTo("secondValue");
        request.KeyConditionExpression.ShouldBeEqualTo("fieldName = :fieldNameValue and name = :nameValue");
    }

    [Fact]
    public void ThenByWithLambdaExpression_KeyConditionExpression()
    {
        var request = QueryRequestBuilder.From("TableName")
            .GlobalIndex("indexName")
            .QueryBy<SampleModel>(x=>x.Id, "someValue")
            .ThenBy<SampleModel>(x=>x.Name, c => c.Equals("secondValue"))
            .Build();
        request.ExpressionAttributeValues[":nameValue"].S.ShouldBeEqualTo("secondValue");
        request.KeyConditionExpression.ShouldBeEqualTo("id = :idValue and name = :nameValue");
    }
}

class SampleModel
{
    public string Id { get; set; }
    public string Name { get; set; }
}