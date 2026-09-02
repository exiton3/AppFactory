using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Api.Parsing;
using AppFactory.Framework.Api.Responses;
using AppFactory.Framework.Domain.ServiceResult;
using AppFactory.Framework.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using HttpResponse = AppFactory.Framework.Api.Responses.HttpResponse;

namespace AppFactory.Framework.Api.AspNetCore.Core;

public class EndpointRequestHandler<TRequest, TResponse> : IEndpointRequestHandler<TRequest, TResponse>
    where TRequest : class, new()
    where TResponse : class
{
    private readonly IRequestParser _requestParser;
    private readonly IFunctionProcessor<TRequest, TResponse> _processor;
    private readonly IEndpointResponseMapper<TResponse> _responseMapper;
    private readonly AppFactoryApiOptions _options;
    private readonly ILogger? _logger;

    public EndpointRequestHandler(
        IRequestParser requestParser,
        IFunctionProcessor<TRequest, TResponse> processor,
        IEndpointResponseMapper<TResponse> responseMapper,
        IOptions<AppFactoryApiOptions> options,
        ILogger? logger = null)
    {
        _requestParser = requestParser;
        _processor = processor;
        _responseMapper = responseMapper;
        _options = options.Value;
        _logger = logger;
    }

    public async Task HandleAsync(HttpContext context)
    {
        var requestContext = new AspNetCoreRequestContext(context);
        HttpResponse response;

        try
        {
            var inputRequest = new InputRequest
            {
                Path = requestContext.PathParameters,
                Query = requestContext.QueryParameters,
                Body = requestContext.Body
            };

            var parsedRequest = _requestParser.ParseRequest<TRequest>(inputRequest);

            _logger?.LogTrace($"Processing {typeof(TRequest).Name}");
            var result = await _processor.Process(parsedRequest, context.RequestAborted);

            response = _responseMapper.Map(result);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error processing request: {Message}", ex.Message);

            var errorMessage = _options.IncludeExceptionDetails
                ? ex.ToString()
                : "An unexpected error occurred";

            var errors = new List<Error> { new("INTERNAL_ERROR", errorMessage) };

            response = new HttpResponseBuilder()
                .StatusCode(HttpStatusCode.InternalServerError)
                .ErrorType("InternalServerError")
                .Errors(errors)
                .Body(new ProblemResponse
                {
                    Problem = "Unexpected error",
                    Errors = errors
                })
                .Build();
        }

        await WriteResponse(context, response);
    }

    private static async Task WriteResponse(HttpContext context, HttpResponse response)
    {
        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = response.ContentType;

        foreach (var header in response.Headers)
            context.Response.Headers[header.Key] = header.Value;

        if (!string.IsNullOrEmpty(response.ErrorType))
            context.Response.Headers["x-error-type"] = response.ErrorType;

        if (!string.IsNullOrEmpty(response.Body))
            await context.Response.WriteAsync(response.Body);
    }
}
