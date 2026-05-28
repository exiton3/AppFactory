using Amazon.Lambda.APIGatewayEvents;
using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Domain.ServiceResult;
using AppFactory.Framework.Shared.Serialization;

namespace AppFactory.Framework.Api.Aws;

/// <summary>
/// AWS Lambda API Gateway response builder
/// Implements IHttpResponseBuilder for API Gateway proxy responses
/// </summary>
public class ApiGatewayResponseBuilder : IHttpResponseBuilder
{
    private readonly APIGatewayProxyResponse _response;
    private readonly IJsonSerializer _jsonSerializer;
    private string _problemTitle;
    private IEnumerable<Error> _errors;

    public ApiGatewayResponseBuilder(IJsonSerializer jsonSerializer = null)
    {
        _jsonSerializer = jsonSerializer ?? new DefaultJsonSerializer();
        _response = new APIGatewayProxyResponse
        {
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, POST, PUT, DELETE, GET, HEAD" }
            }
        };
    }

    public IHttpResponseBuilder StatusCode(int statusCode)
    {
        _response.StatusCode = statusCode;
        return this;
    }

    public IHttpResponseBuilder Header(string key, string value)
    {
        _response.Headers[key] = value;
        return this;
    }

    public IHttpResponseBuilder Headers(IDictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            _response.Headers[header.Key] = header.Value;
        }
        return this;
    }

    public IHttpResponseBuilder Body(string body)
    {
        _response.Body = body;
        return this;
    }

    public IHttpResponseBuilder Body<T>(T data)
    {
        _response.Body = _jsonSerializer.Serialize(data);
        return this;
    }

    public IHttpResponseBuilder ContentType(string contentType)
    {
        _response.Headers["Content-Type"] = contentType;
        return this;
    }

    public IHttpResponseBuilder ErrorType(string errorType)
    {
        _response.Headers["x-amzn-ErrorType"] = errorType;
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
        return _response;
    }
}
