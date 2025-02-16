using System.Net;
using System.Net.Mime;
using AppFactory.Framework.Shared.ServiceClient;
using AppFactory.Framework.TestExtensions;
using Xunit;
using Xunit.Abstractions;

namespace AppFactory.Framework.Infrastructure.UnitTests.ServiceClient;

public class HttpRequestMessageBuilderTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Post_SetHttpMethodAndRequestUri()
    {
        var expectedUrl = @"http://someurl/";
        var message = HttpRequestMessageBuilder
            .Post(expectedUrl)
            .Json()
            .Build();

       message.RequestUri?.AbsoluteUri.ShouldBeEqualTo(expectedUrl);
       message.Method.ShouldBeEqualTo(HttpMethod.Post);
    }

    [Fact]
    public void Json_SetContentTypeToJson()
    {
        var expectedUrl = @"http://someurl/";
        var message = HttpRequestMessageBuilder
            .Post(expectedUrl)
            .Json()
            .Build();
        
        message.Content.Headers.ContentType.MediaType.ShouldBeEqualTo("application/json");
    }

    [Fact]
    public void Xml_SetContentTypeToXml()
    {
        var expectedUrl = @"http://someurl/";
        var message = HttpRequestMessageBuilder
            .Post(expectedUrl)
            .Xml()
            .Build();

        message.Content.Headers.ContentType.MediaType.ShouldBeEqualTo("text/xml");
    }

    [Fact]
    public void Message_SetsContent()
    {
        var expectedUrl = @"http://someurl/";
        var expectedMessage = "someMessageToSend";
        var message = HttpRequestMessageBuilder
            .Post(expectedUrl)
            .Message(expectedMessage)
            .Build();

        var actualContent = message.Content.ReadAsStringAsync().Result;

        actualContent.ShouldBeEqualTo(expectedMessage);
    }

    [Fact]
    public void AddHeader_ShouldAddHeader()
    {
        var expectedUrl = @"http://someurl/";
        var name = "name";
        var value = "value";
        var message = HttpRequestMessageBuilder
            .Post(expectedUrl)
            .AddHeader(name,value)
            .Build();

        message.Headers.Contains(name).ShouldBeTrue();
        message.Headers.First(x=>x.Key == name).Value.First().ShouldBeEqualTo(value);
    }

    [Fact]
    public void AcceptHeader_ShouldAddHeaderWithMediaType()
    {
        var expectedUrl = @"http://someurl/";
        var message = HttpRequestMessageBuilder
            .Post(expectedUrl)
            .Accept(MediaTypeNames.Application.Json)
            .Build();

        testOutputHelper.WriteLine(message.ToString());

        message.Headers.Accept.First().MediaType.ShouldBeEqualTo("application/json");
    }

    [Fact]
    public void BearerToken_AddsAuthorizationHeader()
    {
        var expectedUrl = @"http://someurl/";
        var message = HttpRequestMessageBuilder
            .Post(expectedUrl)
            .BearerToken("some_token")
            .Build();

        var authorization = message.Headers.Authorization;

        authorization.Scheme.ShouldBeEqualTo(AuthorizationScheme.Bearer);
        authorization.Parameter.ShouldBeEqualTo("some_token");

        testOutputHelper.WriteLine(message.ToString());
    }

    [Fact]
    public void MessageNotSet_ContentShouldNotBeEmpty()
    {
        var expectedUrl = @"http://someurl/";
        var message = HttpRequestMessageBuilder
            .Post(expectedUrl)
            .BearerToken("some_token")
            .Build();

        message.Content.ShouldBeNotNull();

        testOutputHelper.WriteLine(message.ToString());
    }
}