using Microsoft.AspNetCore.Http;
using AppFactory.Framework.Api.Abstractions;
using HttpMethodEnum = AppFactory.Framework.Api.Abstractions.HttpMethod;

namespace AppFactory.Framework.Api.AspNetCore;

/// <summary>
/// ASP.NET Core HTTP request context adapter
/// Implements IHttpRequestContext for ASP.NET Core HttpContext
/// </summary>
public class AspNetCoreRequestContext : IHttpRequestContext
{
    private readonly HttpContext _httpContext;
    private string _body;

    public AspNetCoreRequestContext(HttpContext httpContext)
    {
        _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
    }

    public string RequestId => _httpContext.TraceIdentifier;

    public HttpMethodEnum Method => ParseHttpMethod(_httpContext.Request.Method);

    public IDictionary<string, string> PathParameters
    {
        get
        {
            var parameters = new Dictionary<string, string>();
            
            // Extract route values
            if (_httpContext.Request.RouteValues != null)
            {
                foreach (var kvp in _httpContext.Request.RouteValues)
                {
                    if (kvp.Value != null)
                    {
                        parameters[kvp.Key] = kvp.Value.ToString();
                    }
                }
            }
            
            return parameters;
        }
    }

    public IDictionary<string, string> QueryParameters
    {
        get
        {
            var parameters = new Dictionary<string, string>();
            
            foreach (var kvp in _httpContext.Request.Query)
            {
                parameters[kvp.Key] = kvp.Value.ToString();
            }
            
            return parameters;
        }
    }

    public IDictionary<string, string> Headers
    {
        get
        {
            var headers = new Dictionary<string, string>();
            
            foreach (var header in _httpContext.Request.Headers)
            {
                headers[header.Key] = header.Value.ToString();
            }
            
            return headers;
        }
    }

    public string Body
    {
        get
        {
            if (_body == null)
            {
                _httpContext.Request.EnableBuffering();
                using var reader = new StreamReader(_httpContext.Request.Body, leaveOpen: true);
                _body = reader.ReadToEndAsync().GetAwaiter().GetResult();
                _httpContext.Request.Body.Position = 0;
            }
            return _body;
        }
    }

    public Stream BodyStream
    {
        get
        {
            _httpContext.Request.EnableBuffering();
            return _httpContext.Request.Body;
        }
    }

    public string ContentType => _httpContext.Request.ContentType ?? "application/json";

    public string Path => _httpContext.Request.Path.Value ?? string.Empty;

    public string QueryString => _httpContext.Request.QueryString.Value ?? string.Empty;

    private static HttpMethod ParseHttpMethod(string method)
    {
        return method?.ToUpperInvariant() switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "DELETE" => HttpMethod.Delete,
            "PATCH" => HttpMethod.Patch,
            "HEAD" => HttpMethod.Head,
            "OPTIONS" => HttpMethod.Options,
            _ => throw new ArgumentException($"Unsupported HTTP method: {method}", nameof(method))
        };
    }
}
