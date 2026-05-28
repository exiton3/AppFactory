using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AppFactory.Framework.Api.Abstractions;
using HttpMethodEnum = AppFactory.Framework.Api.Abstractions.HttpMethod;

namespace AppFactory.Framework.Api.Aws;

/// <summary>
/// AWS Lambda API Gateway request context adapter
/// Implements IHttpRequestContext for API Gateway proxy requests
/// </summary>
public class ApiGatewayRequestContext : IHttpRequestContext
{
    private readonly APIGatewayProxyRequest _request;
    private readonly ILambdaContext _lambdaContext;

    public ApiGatewayRequestContext(APIGatewayProxyRequest request, ILambdaContext lambdaContext)
    {
        _request = request ?? throw new ArgumentNullException(nameof(request));
        _lambdaContext = lambdaContext ?? throw new ArgumentNullException(nameof(lambdaContext));
    }

    public string RequestId => _lambdaContext.AwsRequestId;

    public HttpMethod Method => ParseHttpMethod(_request.HttpMethod);

    public IDictionary<string, string> PathParameters => 
        _request.PathParameters ?? new Dictionary<string, string>();

    public IDictionary<string, string> QueryParameters => 
        _request.QueryStringParameters ?? new Dictionary<string, string>();

    public IDictionary<string, string> Headers => 
        _request.Headers ?? new Dictionary<string, string>();

    public string Body => _request.Body ?? string.Empty;

    public Stream BodyStream
    {
        get
        {
            if (string.IsNullOrEmpty(_request.Body))
                return Stream.Null;

            if (_request.IsBase64Encoded)
            {
                var bytes = Convert.FromBase64String(_request.Body);
                return new MemoryStream(bytes);
            }

            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_request.Body));
        }
    }

    public string ContentType => 
        Headers.TryGetValue("Content-Type", out var contentType) 
            ? contentType 
            : "application/json";

    public string Path => _request.Path ?? string.Empty;

    public string QueryString => 
        _request.QueryStringParameters != null && _request.QueryStringParameters.Any()
            ? "?" + string.Join("&", _request.QueryStringParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"))
            : string.Empty;

    private static HttpMethodEnum ParseHttpMethod(string method)
    {
        return method?.ToUpperInvariant() switch
        {
            "GET" => HttpMethodEnum.Get,
            "POST" => HttpMethodEnum.Post,
            "PUT" => HttpMethodEnum.Put,
            "DELETE" => HttpMethodEnum.Delete,
            "PATCH" => HttpMethodEnum.Patch,
            "HEAD" => HttpMethodEnum.Head,
            "OPTIONS" => HttpMethodEnum.Options,
            _ => throw new ArgumentException($"Unsupported HTTP method: {method}", nameof(method))
        };
    }
}
