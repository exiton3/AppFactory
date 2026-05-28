using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using AppFactory.Framework.Api.Core;
using AppFactory.Framework.DependencyInjection;

namespace AppFactory.Framework.Api.Azure;

/// <summary>
/// Base class for Azure Functions with HTTP trigger
/// Provides CQRS-based request handling with automatic parsing and response building
/// Uses Azure Functions isolated worker model (v4)
/// </summary>
/// <typeparam name="TRequest">Command or Query request model</typeparam>
/// <typeparam name="TResponse">Response DTO model</typeparam>
public abstract class AzureFunctionHandlerBase<TRequest, TResponse> 
    where TRequest : class, new() 
    where TResponse : class
{
    private readonly FunctionHandlerCore<TRequest, TResponse> _core;

    protected AzureFunctionHandlerBase(IStartup startup = null)
    {
        _core = new FunctionHandlerCore<TRequest, TResponse>(startup ?? GetStartup());
    }

    /// <summary>
    /// Handle HTTP request from Azure Functions
    /// </summary>
    /// <param name="req">HTTP request data</param>
    /// <param name="executionContext">Function execution context</param>
    /// <returns>HTTP response data</returns>
    protected async Task<HttpResponseData> Handle(
        HttpRequestData req,
        FunctionContext executionContext)
    {
        var requestContext = new HttpRequestDataContext(req, executionContext);
        var responseBuilder = new HttpResponseDataBuilder(req);

        await _core.HandleRequest(requestContext, responseBuilder);

        return (HttpResponseData)responseBuilder.Build();
    }

    /// <summary>
    /// Override to provide custom startup configuration
    /// </summary>
    protected abstract IStartup GetStartup();
}
