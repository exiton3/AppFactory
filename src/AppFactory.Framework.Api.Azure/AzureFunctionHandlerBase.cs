using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using AppFactory.Framework.Api.Core;
using AppFactory.Framework.Api.Responses;
using AppFactory.Framework.DependencyInjection;
using SystemHttpStatusCode = System.Net.HttpStatusCode;

namespace AppFactory.Framework.Api.Azure;

public abstract class AzureFunctionHandlerBase<TRequest, TResponse>
    where TRequest : class, new()
    where TResponse : class
{
    private readonly FunctionHandlerCore<TRequest, TResponse> _core;

    protected AzureFunctionHandlerBase(IStartup startup = null)
    {
        _core = new FunctionHandlerCore<TRequest, TResponse>(startup ?? GetStartup());
    }

    protected async Task<HttpResponseData> Handle(
        HttpRequestData req,
        FunctionContext executionContext)
    {
        var requestContext = new HttpRequestDataContext(req, executionContext);
        var response = await _core.HandleRequest(requestContext);
        return ToHttpResponseData(req, response);
    }

    protected abstract IStartup GetStartup();

    private static HttpResponseData ToHttpResponseData(HttpRequestData request, HttpResponse response)
    {
        var httpResponse = request.CreateResponse();
        httpResponse.StatusCode = (SystemHttpStatusCode)response.StatusCode;

        httpResponse.Headers.Add("Content-Type", response.ContentType);
        httpResponse.Headers.Add("Access-Control-Allow-Origin", "*");
        httpResponse.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, POST, PUT, DELETE, GET, HEAD");

        foreach (var header in response.Headers)
        {
            if (httpResponse.Headers.Contains(header.Key))
                httpResponse.Headers.Remove(header.Key);
            httpResponse.Headers.Add(header.Key, header.Value);
        }

        if (!string.IsNullOrEmpty(response.ErrorType))
            httpResponse.Headers.Add("x-error-type", response.ErrorType);

        if (!string.IsNullOrEmpty(response.Body))
            httpResponse.WriteString(response.Body);

        return httpResponse;
    }
}
