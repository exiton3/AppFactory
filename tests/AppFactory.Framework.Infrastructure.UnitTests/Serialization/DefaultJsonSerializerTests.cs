using AppFactory.Framework.Shared.Serialization;
using AppFactory.Framework.TestExtensions;
using Xunit;

namespace AppFactory.Framework.Infrastructure.UnitTests.Serialization;

public class DefaultJsonSerializerTests
{
    private DefaultJsonSerializer _serializer;

    public DefaultJsonSerializerTests()
    {
        _serializer = new DefaultJsonSerializer();
    }

    [Fact]
    public void ObjectHasNumbers_ShouldSerializeNumericAsNumber()
    {

        var testObj = new TestClass
        {
            Name = "some name",
            Amount = 123.5m
        };

        var result = _serializer.Serialize(testObj);

        result.ShouldContain("\"amount\": 123.5");
    }

    [Fact]
    public void ObjectHasTextWithSpecialCharacters_ShouldSerialize()
    {

        var testObj = new TestClass
        {
            Name = "'some' name",
            Amount = 123.5m
        };

        var result = _serializer.Serialize(testObj);

        result.ShouldContain("\"name\": \"\\u0027some\\u0027 name\"");

    }

    [Fact]
    public void PropertyNamesShouldBeSerializedInCamelCase()
    {

        var testObj = new TestClass
        {
            Name = "'some' name",
            Amount = 123.5m
        };

        var result = _serializer.Serialize(testObj);

        result.ShouldContain("\"amount\"");
        result.ShouldContain("\"name\"");
    }
}

class TestClass
{
    public string Name { get; set; }

    public decimal Amount { get; set; }

}