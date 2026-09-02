using Amazon.Lambda.APIGatewayEvents;
using AppFactory.Framework.Domain.ServiceResult;
using AppFactory.Framework.Shared.Serialization;

namespace AppFactory.Framework.Api.Aws;

[Obsolete("Use HttpResponseBuilder instead. This class will be removed in a future version.")]
public class ApiGatewayResponseBuilder
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

    public ApiGatewayResponseBuilder StatusCode(int statusCode)
    {
        _response.StatusCode = statusCode;
        return this;
    }

    public ApiGatewayResponseBuilder Header(string key, string value)
    {
        _response.Headers[key] = value;
        return this;
    }

    public ApiGatewayResponseBuilder Headers(IDictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            _response.Headers[header.Key] = header.Value;
        }
        return this;
    }

    public ApiGatewayResponseBuilder Body(string body)
    {
        _response.Body = body;
        return this;
    }

    public ApiGatewayResponseBuilder Body<T>(T data)
    {
        _response.Body = _jsonSerializer.Serialize(data);
        return this;
    }

    public ApiGatewayResponseBuilder ContentType(string contentType)
    {
        _response.Headers["Content-Type"] = contentType;
        return this;
    }

    public ApiGatewayResponseBuilder ErrorType(string errorType)
    {
        _response.Headers["x-amzn-ErrorType"] = errorType;
        return this;
    }

    public ApiGatewayResponseBuilder ProblemTitle(string title)
    {
        _problemTitle = title;
        return this;
    }

    public ApiGatewayResponseBuilder Errors(IEnumerable<Error> errors)
    {
        _errors = errors;
        return this;
    }

    public object Build()
    {
        return _response;
    }
}
