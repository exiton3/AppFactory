using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace AppFactory.Framework.Shared.ServiceClient;

public class HttpRequestMessageBuilder
{
    private readonly HttpRequestMessage _requestMessage;
    private string _contentType;
    private string _messageToSend;

    private HttpRequestMessageBuilder(HttpRequestMessage message)
    {
        _contentType = MediaTypeNames.Text.Plain;
        _messageToSend = string.Empty;
        _requestMessage = message;
    }

    public HttpRequestMessageBuilder AddHeader(string name, string value)
    {
        _requestMessage.Headers.Add(name, value);

        return this;
    }

    public HttpRequestMessageBuilder Accept(string mediaType)
    {
        _requestMessage.Headers.Accept.Add( new MediaTypeWithQualityHeaderValue(mediaType));

        return this;
    }

    public HttpRequestMessageBuilder Authorization(string scheme, string value)
    {
        _requestMessage.Headers.Authorization = new AuthenticationHeaderValue(scheme, value);

        return this;
    }

    /// <summary>
    /// Adds Authorization header with Bearer Authorization Schema
    /// </summary>
    /// <param name="token">Authorization token</param>
    /// <returns>HttpRequestMessageBuilder</returns>
    public HttpRequestMessageBuilder BearerToken(string token)
    {
        Authorization(AuthorizationScheme.Bearer, token);

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
        _contentType = MediaTypeNames.Application.Json; 

        return this;
    }

    public HttpRequestMessageBuilder Xml()
    {
        _contentType = MediaTypeNames.Text.Xml;

        return this;
    }

    public HttpRequestMessage Build()
    {
        if (!string.IsNullOrEmpty(_messageToSend))
        {
            _requestMessage.Content = new StringContent(_messageToSend, Encoding.UTF8, _contentType);
        }

        return _requestMessage;
    }

    public HttpRequestMessageBuilder Url(string url)
    {
        throw new NotImplementedException();
    }
}