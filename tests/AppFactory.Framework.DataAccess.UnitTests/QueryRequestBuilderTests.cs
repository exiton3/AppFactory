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

}