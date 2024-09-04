using AppFactory.Framework.TestExtensions;
using Xunit;

namespace AppFactory.Framework.DataAccess.UnitTests;


class TestModelConfig:IModelConfig<TestModel>
{
    public void Configure(IModelConfigOptions<TestModel> config)
    {
        config
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
    private readonly DynamoDbModelConfig<TestModel> _config;

    public DynamoDBModelConfigBuilderTests()
    {
        _config = new DynamoDbModelConfig<TestModel>();
    }

    [Fact]
    public void PK_ShouldBeSet()
    {
        TestModel model = new TestModel
        {
            Id = "1234"
        };

        _config
            .PKPrefix("PK")
            .SKPrefix("SK")
            .Id(x => x.Id);
        var primaryKey = _config.GetPrimaryKey(model);

        primaryKey.PK.ShouldBeEqualTo("PK#1234");
        primaryKey.SK.ShouldBeEqualTo("SK#1234");
    }

    [Fact]
    public void ConfigureModelConfig_ShouldSetPrimaryKeyProperties()
    {
        TestModel model = new TestModel
        {
            Id = "1234"
        };

        var modelConfig = new TestModelConfig();

        modelConfig.Configure(_config);

        var primaryKey = _config.GetPrimaryKey(model);

        primaryKey.PK.ShouldBeEqualTo("PK#1234");
        primaryKey.SK.ShouldBeEqualTo("SK#1234");
    }

    [Fact]
    public void PrefixesNotSet_PKShouldBeEqualsToId()
    {
        TestModel model = new TestModel
        {
            Id = "1234"
        };

        _config
            .Id(x => x.Id);

        _config.GetPrimaryKey(model).PK.ShouldBeEqualTo("1234");
        _config.GetPrimaryKey(model).SK.ShouldBeEqualTo("1234");
    }

    [Fact]
    public void SK_ShouldBeSetWithPrefix()
    {
        var model = new TestModel
        {
            Id = "1234"
        };

         _config
            .SKPrefix("SK")
            .Id(x => x.Id);

        _config.GetPrimaryKey(model).SK.ShouldBeEqualTo("SK#1234");
    }


    [Fact]
    public void GetPrimaryKey_ShouldBeSet()
    {
        var model = new TestModel
        {
            Id = "1234"
        };

        _config
            .SKPrefix("SK")
            .Id(x => x.Id);

        var primaryKey = _config.GetPrimaryKey(model);

        primaryKey.PK.ShouldBeEqualTo("1234");
        primaryKey.SK.ShouldBeEqualTo("SK#1234");
    }

    [Fact]
    public void GetPrimaryKey_ById_ShouldReturnSet()
    {
        var model = new TestModel
        {
            Id = "1234"
        };

         _config
            .SKPrefix("SK")
            .Id(x => x.Id);

        var primaryKey = _config.GetPrimaryKey(model.Id);

        primaryKey.PK.ShouldBeEqualTo("1234");
        primaryKey.SK.ShouldBeEqualTo("SK#1234");
    }
}