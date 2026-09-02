using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Api.Responses;
using AppFactory.Framework.Domain.ServiceResult;
using AppFactory.Framework.Shared.Serialization;

namespace AppFactory.Framework.Api;

public class HttpResponseBuilder : IHttpResponseBuilder
{
    private readonly IJsonSerializer _jsonSerializer;
    private int _statusCode = HttpStatusCode.OK;
    private string _body;
    private string _contentType = "application/json";
    private readonly Dictionary<string, string> _headers = new();
    private string _errorType;
    private string _problemTitle;
    private IEnumerable<Error> _errors;

    public HttpResponseBuilder(IJsonSerializer jsonSerializer = null)
    {
        _jsonSerializer = jsonSerializer ?? new DefaultJsonSerializer();
    }

    public IHttpResponseBuilder StatusCode(int statusCode)
    {
        _statusCode = statusCode;
        return this;
    }

    public IHttpResponseBuilder Header(string key, string value)
    {
        _headers[key] = value;
        return this;
    }

    public IHttpResponseBuilder Headers(IDictionary<string, string> headers)
    {
        foreach (var header in headers)
            _headers[header.Key] = header.Value;
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
        _contentType = contentType;
        return this;
    }

    public IHttpResponseBuilder ErrorType(string errorType)
    {
        _errorType = errorType;
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

    public HttpResponse Build()
    {
        return new HttpResponse
        {
            StatusCode = _statusCode,
            Body = _body,
            ContentType = _contentType,
            Headers = new Dictionary<string, string>(_headers),
            ErrorType = _errorType,
            ProblemTitle = _problemTitle,
            Errors = _errors
        };
    }
}
