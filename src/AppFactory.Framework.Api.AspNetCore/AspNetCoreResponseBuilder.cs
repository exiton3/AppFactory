using Microsoft.AspNetCore.Http;
using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Api.Responses;
using AppFactory.Framework.Domain.ServiceResult;
using AppFactory.Framework.Shared.Serialization;

namespace AppFactory.Framework.Api.AspNetCore;

/// <summary>
/// ASP.NET Core HTTP response builder
/// Implements IHttpResponseBuilder for ASP.NET Core IResult
/// </summary>
public class AspNetCoreResponseBuilder : IHttpResponseBuilder
{
    private readonly HttpContext _httpContext;
    private readonly IJsonSerializer _jsonSerializer;
    private int _statusCode = 200;
    private string _body;
    private string _contentType = "application/json";
    private readonly Dictionary<string, string> _headers = new();
    private string _problemTitle;
    private IEnumerable<Error> _errors;

    public AspNetCoreResponseBuilder(HttpContext httpContext, IJsonSerializer jsonSerializer = null)
    {
        _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        _jsonSerializer = jsonSerializer ?? new DefaultJsonSerializer();
        
        // Default CORS headers
        _headers["Access-Control-Allow-Origin"] = "*";
        _headers["Access-Control-Allow-Methods"] = "OPTIONS, POST, PUT, DELETE, GET, HEAD";
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
        {
            _headers[header.Key] = header.Value;
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
        _contentType = contentType;
        return this;
    }

    public IHttpResponseBuilder ErrorType(string errorType)
    {
        _headers["x-error-type"] = errorType;
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
        // Set status code
        _httpContext.Response.StatusCode = _statusCode;
        
        // Set headers
        foreach (var header in _headers)
        {
            _httpContext.Response.Headers[header.Key] = header.Value;
        }
        
        // Set content type
        _httpContext.Response.ContentType = _contentType;
        
        // Write body
        if (!string.IsNullOrEmpty(_body))
        {
            _httpContext.Response.WriteAsync(_body).GetAwaiter().GetResult();
        }
        
        return Results.Empty;
    }
}
