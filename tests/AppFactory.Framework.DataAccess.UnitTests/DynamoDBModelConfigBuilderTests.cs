using AppFactory.Framework.DataAccess.AmazonDbServices;
using AppFactory.Framework.DataAccess.Configuration;
using AppFactory.Framework.DataAccess.Queries;
using AppFactory.Framework.Logging;
using AppFactory.Framework.TestExtensions;
using Xunit;

namespace AppFactory.Framework.DataAccess.UnitTests;


class TestRepository:Repository2<TestModel>
{
    public TestRepository(IDynamoDBClientFactory dynamoDbFactory, IAWSSettings awsSettings, ILogger logger) : base(dynamoDbFactory, awsSettings, logger)
    {
    }

    protected override void Configure(DynamoDBModelConfigBuilder<TestModel> builder)
    {
        builder
            .SKPrefix("SK")
            .PKPrefix("PK")
            .Id(x => x.Id);
    }
}

internal class TestModel
{

    public string Id { get; set; }
}

public class DynamoDBModelConfigBuilderTests
{
    private DynamoDBModelConfigBuilder<TestModel> _builder;

    public DynamoDBModelConfigBuilderTests()
    {
        _builder = new DynamoDBModelConfigBuilder<TestModel>();
    }

    [Fact]
    public void SetsPK_and_SK_PrefixesCorrectly()
    {

        _builder = _builder
            .PKPrefix("PK")
            .SKPrefix("SK");

        var config = _builder.Build();

        config.PKPrefix.ShouldBeEqualTo("PK");
        config.SKPrefix.ShouldBeEqualTo("SK");
    }

    [Fact]
    public void PK_ShouldBeSet()
    {

        TestModel model = new TestModel
        {
            Id = "1234"
        };

        _builder = _builder
            .SKPrefix("SK")
            .Id(x => x.Id);

        var config = _builder.Build();

        config.GetPKValue(model).ShouldBeEqualTo("1234");
    }

    [Fact]
    public void SK_ShouldBeSetWithPrefix()
    {

        var model = new TestModel
        {
            Id = "1234"
        };

        _builder = _builder
            .SKPrefix("SK")
            .Id(x => x.Id);

        var config = _builder.Build();

        config.GetSKValue(model).ShouldBeEqualTo("SK#1234");
    }


    [Fact]
    public void GetPrimaryKey_ShouldBeSet()
    {
        var model = new TestModel
        {
            Id = "1234"
        };

        _builder = _builder
            .SKPrefix("SK")
            .Id(x => x.Id);

        var config = _builder.Build();

        config.GetPrimaryKey(model).PK.ShouldBeEqualTo("1234");
        config.GetPrimaryKey(model).SK.ShouldBeEqualTo("SK#1234");
    }

    [Fact]
    public void GetPrimaryKey_ById_ShouldReturnSet()
    {
        var model = new TestModel
        {
            Id = "1234"
        };

        _builder = _builder
            .SKPrefix("SK")
            .Id(x => x.Id);

        var config = _builder.Build();
        var primaryKey = config.GetPrimaryKey(model.Id);

        primaryKey.PK.ShouldBeEqualTo("1234");
        primaryKey.SK.ShouldBeEqualTo("SK#1234");
    }
}