using Amazon.Lambda.APIGatewayEvents;
using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Domain.ServiceResult;
using AppFactory.Framework.TestExtensions;

namespace AppFactory.Framework.Api.Aws.UnitTests;

public class ApiGatewayResponseBuilderTests
{
    [Fact]
    public void StatusCode_SetsCorrectly()
    {
        var builder = new ApiGatewayResponseBuilder();

        builder.StatusCode(200);
        var response = (APIGatewayProxyResponse)builder.Build();

        response.StatusCode.ShouldBe(200);
    }

    [Fact]
    public void Header_AddsHeader()
    {
        var builder = new ApiGatewayResponseBuilder();

        builder.Header("X-Custom", "value");
        var response = (APIGatewayProxyResponse)builder.Build();

        response.Headers["X-Custom"].ShouldBe("value");
    }

    [Fact]
    public void Headers_AddsMultipleHeaders()
    {
        var builder = new ApiGatewayResponseBuilder();
        var headers = new Dictionary<string, string>
        {
            { "X-Custom-1", "value1" },
            { "X-Custom-2", "value2" }
        };

        builder.Headers(headers);
        var response = (APIGatewayProxyResponse)builder.Build();

        response.Headers["X-Custom-1"].ShouldBe("value1");
        response.Headers["X-Custom-2"].ShouldBe("value2");
    }

    [Fact]
    public void Body_StringBody_SetsCorrectly()
    {
        var builder = new ApiGatewayResponseBuilder();

        builder.Body("{\"message\":\"test\"}");
        var response = (APIGatewayProxyResponse)builder.Build();

        response.Body.ShouldBe("{\"message\":\"test\"}");
    }

    [Fact]
    public void Body_ObjectBody_SerializesCorrectly()
    {
        var builder = new ApiGatewayResponseBuilder();
        var data = new { Message = "test" };

        builder.Body(data);
        var response = (APIGatewayProxyResponse)builder.Build();

        response.Body.ShouldContain("\"Message\"");
        response.Body.ShouldContain("\"test\"");
    }

    [Fact]
    public void ContentType_SetsCorrectly()
    {
        var builder = new ApiGatewayResponseBuilder();

        builder.ContentType("text/plain");
        var response = (APIGatewayProxyResponse)builder.Build();

        response.Headers["Content-Type"].ShouldBe("text/plain");
    }

    [Fact]
    public void ErrorType_SetsCustomHeader()
    {
        var builder = new ApiGatewayResponseBuilder();

        builder.ErrorType("ValidationException");
        var response = (APIGatewayProxyResponse)builder.Build();

        response.Headers["x-amzn-ErrorType"].ShouldBe("ValidationException");
    }

    [Fact]
    public void Build_DefaultHeaders_IncludesCorsHeaders()
    {
        var builder = new ApiGatewayResponseBuilder();

        var response = (APIGatewayProxyResponse)builder.Build();

        response.Headers["Content-Type"].ShouldBe("application/json");
        response.Headers["Access-Control-Allow-Origin"].ShouldBe("*");
        response.Headers["Access-Control-Allow-Methods"].ShouldContain("GET");
    }

    [Fact]
    public void Errors_WithErrorList_StoresErrors()
    {
        var builder = new ApiGatewayResponseBuilder();
        var errors = new List<Error>
        {
            new Error("CODE1", "Message 1"),
            new Error("CODE2", "Message 2")
        };

        builder.Errors(errors);
        var response = (APIGatewayProxyResponse)builder.Build();

        response.ShouldNotBeNull();
    }

    [Fact]
    public void FluentApi_ChainsCorrectly()
    {
        var builder = new ApiGatewayResponseBuilder();

        var result = builder
            .StatusCode(201)
            .ContentType("application/json")
            .Body("{\"id\":\"123\"}")
            .Header("X-Request-Id", "abc")
            .Build();

        result.ShouldNotBeNull();
        var response = (APIGatewayProxyResponse)result;
        response.StatusCode.ShouldBe(201);
        response.Body.ShouldBe("{\"id\":\"123\"}");
    }
}
