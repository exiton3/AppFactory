using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AppFactory.Framework.Api.Core;
using AppFactory.Framework.DependencyInjection;

namespace AppFactory.Framework.Api.Aws;

/// <summary>
/// Base class for AWS Lambda functions with API Gateway integration
/// Provides CQRS-based request handling with automatic parsing and response building
/// </summary>
/// <typeparam name="TRequest">Command or Query request model</typeparam>
/// <typeparam name="TResponse">Response DTO model</typeparam>
public abstract class LambdaFunctionHandlerBase<TRequest, TResponse> 
    where TRequest : class, new() 
    where TResponse : class
{
    private readonly FunctionHandlerCore<TRequest, TResponse> _core;

    protected LambdaFunctionHandlerBase(IStartup startup = null)
    {
        _core = new FunctionHandlerCore<TRequest, TResponse>(startup ?? GetStartup());
    }

    /// <summary>
    /// Handle API Gateway proxy request
    /// </summary>
    /// <param name="request">API Gateway proxy request</param>
    /// <param name="context">Lambda context</param>
    /// <returns>API Gateway proxy response</returns>
    public async Task<APIGatewayProxyResponse> Handle(
        APIGatewayProxyRequest request, 
        ILambdaContext context)
    {
        var requestContext = new ApiGatewayRequestContext(request, context);
        var responseBuilder = new ApiGatewayResponseBuilder();

        await _core.HandleRequest(requestContext, responseBuilder);

        return (APIGatewayProxyResponse)responseBuilder.Build();
    }

    /// <summary>
    /// Override to provide custom startup configuration
    /// </summary>
    protected abstract IStartup GetStartup();
}
