using AppFactory.Framework.Api.Parsing;
using AppFactory.Framework.Api.Parsing.Configurations;
using AppFactory.Framework.Api.Parsing.Converters;
using AppFactory.Framework.Api.Parsing.Mappers;
using AppFactory.Framework.Shared.Serialization;
using AppFactory.Framework.TestExtensions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace AppFactory.Framework.Api.UnitTests.Parsing;

public class RequestParserTests
{
    private readonly ITestOutputHelper _output;
    private readonly RequestParser _parser;
    private readonly ParseModelMapRegistry _modelMapRegistry;
    private readonly Mock<IJsonSerializer> _jsonSerializer;

    public RequestParserTests(ITestOutputHelper output)
    {
        _output = output;
        var parseModelMaps = new List<IParseModelMap> { new MyRequestModelMap() };
        _modelMapRegistry = new ParseModelMapRegistry(parseModelMaps);
        _jsonSerializer = new Mock<IJsonSerializer>();
        var propertyMappers = new List<IPropertyMapper>
        {
            new PathPropertyMapper(),
            new QueryPropertyMapper(),
            new BodyPropertyMapper(_jsonSerializer.Object)

        };
        _parser = new RequestParser(_modelMapRegistry,
            new PropertyMapperRegistry(propertyMappers));
    }

    [Fact]
    public void ParseRequest_OnInputPathParameters_ReturnsParsedRequest()
    {
        var @params = new Dictionary<string, string> {
            {"price", "123.4"},
            {"name", "Some name"},
        };

        var result = _parser.ParseRequest<MyRequest>(new InputRequest
        {
            Path = @params,
            Query = @params
        });


        result.Name.ShouldBeEqualTo("Some name");
        result.Price.ShouldBeEqualTo(123.4m);
    }



    [Fact]
    public void ParseRequest_QueryAndPathParamsSet_ReturnsParsedRequest()
    {
        var @params = new Dictionary<string, string> {
            {"id", "12"},
            {"name", "Some name"}
        };

        var query = new Dictionary<string, string> {
            {"id", "12"},
            {"name", "Some name"}
        };
        _modelMapRegistry.AddParseModelMap<RequestTest, MyRequestModelMapWithQuery>();

        var result = _parser.ParseRequest<RequestTest>(new InputRequest
        {
            Path = @params,
            Query = query
        });


        result.Id.ShouldBeEqualTo(12);
        result.Name.ShouldBeEqualTo("Some name");
    }


    [Fact]
    public void ParseRequest_NotRequiredQueryParam_ShouldBeSkipped()
    {
        var emptyQuery = new Dictionary<string, string> ();

        string body = " some body";

        var expectedData = new MyData { Value = "value" };

        _jsonSerializer.Setup(x => x.Deserialize(body, It.IsAny<Type>())).Returns(expectedData);

        _modelMapRegistry.AddParseModelMap<RequestTest, RequestMapWithBody>();

        var result = _parser.ParseRequest<RequestTest>(new InputRequest
        {
            Query = emptyQuery,
            Body = body
        });


         result.Id.ShouldBeEqualTo(0);
        result.Name.ShouldBeEqualTo(default(string));
        result.Data.ShouldBeEqualTo(expectedData);
    }

    [Fact]
    public void FromBodyProperty_ShouldBeMappedUsingJsonDeserialize()
    {
        var emptyQuery = new Dictionary<string, string>();

        string body = " some body";

        var expectedData = new MyData { Value = "value" };

        _jsonSerializer.Setup(x => x.Deserialize(body, It.IsAny<Type>())).Returns(expectedData);

        _modelMapRegistry.AddParseModelMap<RequestTest, RequestMapWithBody>();

        var result = _parser.ParseRequest<RequestTest>(new InputRequest
        {
            Query = emptyQuery,
            Body = body
        });


        result.Id.ShouldBeEqualTo(0);
        result.Name.ShouldBeEqualTo(default(string));
        result.Data.ShouldBeEqualTo(expectedData);
    }

    [Fact]
    public void ParseRequest_InputRequestWithBody_ReturnsParsedRequest()
    {
        var @params = new Dictionary<string, string> {
            {"id", "12"},
            {"name", "Some name"}
        };

        var query = new Dictionary<string, string> {
            {"id", "12"},
            {"name", "Some name"}
        };

        string body = " some body";

        var expectedData = new MyData { Value = "value" };

        _jsonSerializer.Setup(x => x.Deserialize(body, It.IsAny<Type>())).Returns(expectedData);

        _modelMapRegistry.AddParseModelMap<RequestTest, RequestMapWithBody2>();

        var result = _parser.ParseRequest<RequestTest>(new InputRequest
        {
            Path = @params,
            Query = query,
            Body = body
        });


        result.Id.ShouldBeEqualTo(12);
        result.Name.ShouldBeEqualTo("Some name");
        result.Data.ShouldBeEqualTo(expectedData);
    }


    [Fact]
    public void ParseRequest_BodyAndQuerySet_ReturnsParsedRequest()
    {
        var @params = new Dictionary<string, string> {
            {"id", "12"},
            {"name", "Some name"}
        };

        var query = new Dictionary<string, string> {
            {"id", "12"},
            {"name", "Some name"}
        };

        string body = " some body";

        var expectedData = new MyData{ Value = "value"};

        _jsonSerializer.Setup(x => x.Deserialize(body, It.IsAny<Type>())).Returns(expectedData);

        _modelMapRegistry.AddParseModelMap<RequestTest, RequestMapWithBody>();

        var result = _parser.ParseRequest<RequestTest>(new InputRequest
        {
            Path = @params,
            Query = query,
            Body = body
        });


        result.Id.ShouldBeEqualTo(12);
        result.Name.ShouldBeEqualTo("Some name");
        result.Data.ShouldBeEqualTo(expectedData);
    }


    [Fact]
    public void BodyWithContentTypeText_ReturnsParsedRequest()
    {
        string body = "some text body";

        var expectedData = new MyData { Value = "value" };

        _jsonSerializer.Setup(x => x.Deserialize(body, It.IsAny<Type>())).Returns(expectedData);

        _modelMapRegistry.AddParseModelMap<RequestWithBody, RequestMapWithBodyContentType>();

        var result = _parser.ParseRequest<RequestWithBody>(new InputRequest { Body = body });

        result.Data.ShouldBeEqualTo(body);
    }

    [Fact]
    public void RequiredQueryParamIsMissing_ThrowsException()
    {
        var query = new Dictionary<string, string>();

        _modelMapRegistry.AddParseModelMap<RequestTest, RequestMapWithQuery>();

        var inputRequest = new InputRequest
        {
            Query = query,
        };

        var action = () => { _parser.ParseRequest<RequestTest>(inputRequest); };

        var exception = Assert.Throws<RequestParsingException>(action);

        exception.Message.ShouldContain("The query parameter with the name 'name', not found in Query String.");
    }


    [Fact]
    public void RequiredPathParamIsSetButEmpty_ThrowsException()
    {

        var path = new Dictionary<string, string>
        {
            { "id", "12" },
            {"name", ""}
        };

        _modelMapRegistry.AddParseModelMap<RequestTest, RequestMapWithPath>();

        var inputRequest = new InputRequest
        {
            Path = path,
        };

        var action = () => { _parser.ParseRequest<RequestTest>(inputRequest); };

        var exception = Assert.Throws<RequestParsingException>(action);

        exception.Message.ShouldContain("The path parameter 'name', must not be empty.");
    }

    int[] GetMinimumElements(int[] arr, int k)
    {
        var result = new List<int>(arr.Take(k));

        for (int i = k; i < arr.Length; i++)
        {
            int index = FindIndexOfMax(result.ToArray());
            var max = result[index];
            if (arr[i] <= max)
            {
                result[index] = arr[i];
            }
        }

        return result.ToArray();
    }

    int FindIndexOfMax(int[] arr)
    {
        int resultIndex = 0;
        int max = arr[0];
        for (int i = 1; i < arr.Length; i++)
        {
            if (arr[i] >= max)
            {
                max = arr[i];
                resultIndex = i;
            }
        }

        return resultIndex;
    }

    [Fact]
    public void TestGetMinElements()
    {

        int[] arr = { 3, 4, 7, 8, 1, 3, 5, 9 };
        int k = 4;
       var result = GetMinimumElements(arr, k);

       foreach (var item in result)
       {
           Console.Write(item + ", ");
        }
    }

    [Fact]
    public void RequiredPathParamIsMissing_ThrowsException()
    {
        var path = new Dictionary<string, string>();
           
        _modelMapRegistry.AddParseModelMap<RequestTest, RequestMapWithPath>();

        var inputRequest = new InputRequest
        {
            Path = path,
        };

        var action = () => { _parser.ParseRequest<RequestTest>(inputRequest); };

        var exception = Assert.Throws<RequestParsingException>(action);

        exception.Message.ShouldBeEqualTo("The path parameter 'name', not found in Path parameters.");
    }

}

class MyData
{
    public string Value { get; set; }
}
class RequestTest
{
    public int Id { get; set; }

    public string Name { get; set; }

    public MyData Data { get; set; }

}

class RequestWithBody
{
    public string Data { get; set; }
}

class RequestMapWithBody2 : ParseModelMap<RequestTest>
{
    public RequestMapWithBody2()
    {
        Map(x => x.Data).FromBody();
        Map(x => x.Name, "name").FromQuery();
        Map(x => x.Id, "id").FromQuery().UseConverter<StringToIntConverter>();
    }
}


class RequestMapWithBodyContentType : ParseModelMap<RequestWithBody>
{
    public RequestMapWithBodyContentType()
    {
        Map(x => x.Data).FromBody().AsText();
    }
}


class RequestMapWithPath : ParseModelMap<RequestTest>
{
    public RequestMapWithPath()
    {
        Map(x => x.Name, "name").FromPath();
        Map(x => x.Id, "id").FromPath().UseConverter<StringToIntConverter>();
    }
}
class RequestMapWithQuery : ParseModelMap<RequestTest>
{
    public RequestMapWithQuery()
    {
        Map(x => x.Name, "name").FromQuery().Required();
        Map(x => x.Id, "id").FromQuery().UseConverter<StringToIntConverter>();
    }
}
class RequestMapWithBody : ParseModelMap<RequestTest>
{
    public RequestMapWithBody()
    {
        Map(x => x.Data).FromBody();
        Map(x => x.Name, "name").FromQuery();
        Map(x => x.Id, "id").FromQuery().UseConverter<StringToIntConverter>();
    }
}
class MyRequestModelMapWithQuery : ParseModelMap<RequestTest>
{
    public MyRequestModelMapWithQuery()
    {
        Map(x => x.Name, "name").FromQuery();
        Map(x => x.Id, "id").FromPath().UseConverter<StringToIntConverter>();
    }
}