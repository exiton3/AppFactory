using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AppFactory.Framework.Api.Core;
using AppFactory.Framework.Api.Responses;
using AppFactory.Framework.DependencyInjection;

namespace AppFactory.Framework.Api.Aws;

public abstract class LambdaFunctionHandlerBase<TRequest, TResponse>
    where TRequest : class, new()
    where TResponse : class
{
    private readonly FunctionHandlerCore<TRequest, TResponse> _core;

    protected LambdaFunctionHandlerBase(IStartup startup = null)
    {
        _core = new FunctionHandlerCore<TRequest, TResponse>(startup ?? GetStartup());
    }

    public async Task<APIGatewayProxyResponse> Handle(
        APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        var requestContext = new ApiGatewayRequestContext(request, context);
        var response = await _core.HandleRequest(requestContext);
        return ToApiGatewayResponse(response);
    }

    protected abstract IStartup GetStartup();

    private static APIGatewayProxyResponse ToApiGatewayResponse(HttpResponse response)
    {
        var headers = new Dictionary<string, string>
        {
            { "Content-Type", response.ContentType },
            { "Access-Control-Allow-Origin", "*" },
            { "Access-Control-Allow-Methods", "OPTIONS, POST, PUT, DELETE, GET, HEAD" }
        };

        foreach (var header in response.Headers)
            headers[header.Key] = header.Value;

        if (!string.IsNullOrEmpty(response.ErrorType))
            headers["x-amzn-ErrorType"] = response.ErrorType;

        return new APIGatewayProxyResponse
        {
            StatusCode = response.StatusCode,
            Headers = headers,
            Body = response.Body
        };
    }
}
