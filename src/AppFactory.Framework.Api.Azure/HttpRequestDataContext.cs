using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using AppFactory.Framework.Api.Abstractions;

namespace AppFactory.Framework.Api.Azure;

/// <summary>
/// Azure Functions HTTP request context adapter
/// Implements IHttpRequestContext for Azure Functions isolated worker model
/// </summary>
public class HttpRequestDataContext : IHttpRequestContext
{
    private readonly HttpRequestData _request;
    private readonly FunctionContext _functionContext;
    private string _body;

    public HttpRequestDataContext(HttpRequestData request, FunctionContext functionContext)
    {
        _request = request ?? throw new ArgumentNullException(nameof(request));
        _functionContext = functionContext ?? throw new ArgumentNullException(nameof(functionContext));
    }

    public string RequestId => _functionContext.InvocationId;

    public HttpMethod Method => ParseHttpMethod(_request.Method);

    public IDictionary<string, string> PathParameters
    {
        get
        {
            var parameters = new Dictionary<string, string>();
            
            // Extract route parameters from FunctionContext
            if (_functionContext.BindingContext.BindingData != null)
            {
                foreach (var kvp in _functionContext.BindingContext.BindingData)
                {
                    if (kvp.Value != null && kvp.Key != "sys" && kvp.Key != "$request")
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
            
            if (_request.Query != null)
            {
                foreach (var key in _request.Query.AllKeys)
                {
                    parameters[key] = _request.Query[key];
                }
            }
            
            return parameters;
        }
    }

    public IDictionary<string, string> Headers
    {
        get
        {
            var headers = new Dictionary<string, string>();
            
            foreach (var header in _request.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
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
                using var reader = new StreamReader(_request.Body);
                _body = reader.ReadToEnd();
                _request.Body.Position = 0; // Reset for potential re-reading
            }
            return _body;
        }
    }

    public Stream BodyStream => _request.Body;

    public string ContentType => 
        _request.Headers.TryGetValues("Content-Type", out var values)
            ? values.FirstOrDefault() ?? "application/json"
            : "application/json";

    public string Path => _request.Url.AbsolutePath;

    public string QueryString => _request.Url.Query;

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
