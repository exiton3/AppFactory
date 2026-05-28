using Microsoft.Azure.Functions.Worker.Http;
using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Domain.ServiceResult;
using AppFactory.Framework.Shared.Serialization;
using SystemHttpStatusCode = System.Net.HttpStatusCode;

namespace AppFactory.Framework.Api.Azure;

/// <summary>
/// Azure Functions HTTP response builder
/// Implements IHttpResponseBuilder for HttpResponseData
/// </summary>
public class HttpResponseDataBuilder : IHttpResponseBuilder
{
    private readonly HttpRequestData _request;
    private readonly IJsonSerializer _jsonSerializer;
    private HttpResponseData _response;
    private string _body;
    private string _problemTitle;
    private IEnumerable<Error> _errors;

    public HttpResponseDataBuilder(HttpRequestData request, IJsonSerializer jsonSerializer = null)
    {
        _request = request ?? throw new ArgumentNullException(nameof(request));
        _jsonSerializer = jsonSerializer ?? new DefaultJsonSerializer();
        _response = _request.CreateResponse();
        
        // Default headers
        _response.Headers.Add("Content-Type", "application/json");
        _response.Headers.Add("Access-Control-Allow-Origin", "*");
        _response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, POST, PUT, DELETE, GET, HEAD");
    }

    public IHttpResponseBuilder StatusCode(int statusCode)
    {
        _response.StatusCode = (SystemHttpStatusCode)statusCode;
        return this;
    }

    public IHttpResponseBuilder Header(string key, string value)
    {
        if (_response.Headers.Contains(key))
        {
            _response.Headers.Remove(key);
        }
        _response.Headers.Add(key, value);
        return this;
    }

    public IHttpResponseBuilder Headers(IDictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            Header(header.Key, header.Value);
        }
        return this;
    }

    public IHttpResponseBuilder Body(string body)
    {
        _body = body;
        return this;
    }

    public IHttpResponseBuilder Body<T>(T data)
    {
        _body = _jsonSerializer.Serialize(data);
        return this;
    }

    public IHttpResponseBuilder ContentType(string contentType)
    {
        Header("Content-Type", contentType);
        return this;
    }

    public IHttpResponseBuilder ErrorType(string errorType)
    {
        Header("x-error-type", errorType);
        return this;
    }

    public IHttpResponseBuilder ProblemTitle(string title)
    {
        _problemTitle = title;
        return this;
    }

    public IHttpResponseBuilder Errors(IEnumerable<Error> errors)
    {
        _errors = errors;
        return this;
    }

    public object Build()
    {
        if (!string.IsNullOrEmpty(_body))
        {
            _response.WriteString(_body);
        }
        
        return _response;
    }
}
