using System.Text;

namespace AppFactory.Framework.Infrastructure.ServiceClient;

public class HttpRequestMessageBuilder
{
    private readonly HttpRequestMessage _requestMessage;
    private string _contentType;
    private string _messageToSend;

    private HttpRequestMessageBuilder(HttpRequestMessage message)
    {
        _contentType = "text/plain";
        _messageToSend = string.Empty;
        _requestMessage = message;
    }

    public HttpRequestMessageBuilder AddHeader(string name, string value)
    {
        _requestMessage.Headers.Add(name, value);

        return this;
    }

    public static HttpRequestMessageBuilder Post(string url)
    {
        return new HttpRequestMessageBuilder(new HttpRequestMessage(HttpMethod.Post, url));
    }

    public HttpRequestMessageBuilder Message(string message)
    {
        _messageToSend = message;

        return this;
    }

    public HttpRequestMessageBuilder Json()
    {
        _contentType = "application/json";

        return this;
    }

    public HttpRequestMessageBuilder Xml()
    {
        _contentType = "text/xml";

        return this;
    }

    public HttpRequestMessage Build()
    {
        _requestMessage.Content = new StringContent(_messageToSend, Encoding.UTF8, _contentType);

        return _requestMessage;
    }

    public HttpRequestMessageBuilder Url(string url)
    {
        throw new NotImplementedException();
    }
}