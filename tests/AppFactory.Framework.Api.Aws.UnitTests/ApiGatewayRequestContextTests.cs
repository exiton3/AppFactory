using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.TestExtensions;

namespace AppFactory.Framework.Api.Aws.UnitTests;

public class ApiGatewayRequestContextTests
{
    [Fact]
    public void Constructor_ValidInputs_SetsProperties()
    {
        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "POST",
            Path = "/api/users",
            Body = "{\"name\":\"test\"}",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            PathParameters = new Dictionary<string, string> { { "id", "123" } },
            QueryStringParameters = new Dictionary<string, string> { { "page", "1" } }
        };
        var lambdaContext = new TestLambdaContext { AwsRequestId = "test-request-id" };

        var context = new ApiGatewayRequestContext(request, lambdaContext);

        context.RequestId.ShouldBe("test-request-id");
        context.Method.ShouldBe(HttpMethod.Post);
        context.Path.ShouldBe("/api/users");
        context.Body.ShouldBe("{\"name\":\"test\"}");
        context.PathParameters["id"].ShouldBe("123");
        context.QueryParameters["page"].ShouldBe("1");
        context.ContentType.ShouldBe("application/json");
    }

    [Theory]
    [InlineData("GET", HttpMethod.Get)]
    [InlineData("POST", HttpMethod.Post)]
    [InlineData("PUT", HttpMethod.Put)]
    [InlineData("DELETE", HttpMethod.Delete)]
    [InlineData("PATCH", HttpMethod.Patch)]
    public void Method_VariousHttpMethods_ParsesCorrectly(string httpMethod, HttpMethod expected)
    {
        var request = new APIGatewayProxyRequest { HttpMethod = httpMethod };
        var lambdaContext = new TestLambdaContext();

        var context = new ApiGatewayRequestContext(request, lambdaContext);

        context.Method.ShouldBe(expected);
    }

    [Fact]
    public void PathParameters_NullInRequest_ReturnsEmptyDictionary()
    {
        var request = new APIGatewayProxyRequest { PathParameters = null };
        var lambdaContext = new TestLambdaContext();

        var context = new ApiGatewayRequestContext(request, lambdaContext);

        context.PathParameters.ShouldNotBeNull();
        context.PathParameters.Count.ShouldBe(0);
    }

    [Fact]
    public void QueryParameters_NullInRequest_ReturnsEmptyDictionary()
    {
        var request = new APIGatewayProxyRequest { QueryStringParameters = null };
        var lambdaContext = new TestLambdaContext();

        var context = new ApiGatewayRequestContext(request, lambdaContext);

        context.QueryParameters.ShouldNotBeNull();
        context.QueryParameters.Count.ShouldBe(0);
    }

    [Fact]
    public void BodyStream_Base64EncodedBody_DecodesCorrectly()
    {
        var originalText = "Hello World";
        var base64Text = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(originalText));
        var request = new APIGatewayProxyRequest
        {
            Body = base64Text,
            IsBase64Encoded = true
        };
        var lambdaContext = new TestLambdaContext();

        var context = new ApiGatewayRequestContext(request, lambdaContext);

        using var reader = new StreamReader(context.BodyStream);
        var decoded = reader.ReadToEnd();
        decoded.ShouldBe(originalText);
    }

    [Fact]
    public void ContentType_NoContentTypeHeader_ReturnsDefaultJson()
    {
        var request = new APIGatewayProxyRequest { Headers = new Dictionary<string, string>() };
        var lambdaContext = new TestLambdaContext();

        var context = new ApiGatewayRequestContext(request, lambdaContext);

        context.ContentType.ShouldBe("application/json");
    }

    [Fact]
    public void QueryString_WithParameters_BuildsCorrectly()
    {
        var request = new APIGatewayProxyRequest
        {
            QueryStringParameters = new Dictionary<string, string>
            {
                { "page", "1" },
                { "size", "10" }
            }
        };
        var lambdaContext = new TestLambdaContext();

        var context = new ApiGatewayRequestContext(request, lambdaContext);

        context.QueryString.ShouldContain("page=1");
        context.QueryString.ShouldContain("size=10");
        context.QueryString.ShouldStartWith("?");
    }
}
