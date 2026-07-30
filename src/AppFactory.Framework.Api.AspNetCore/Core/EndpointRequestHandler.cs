using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Api.Parsing;
using AppFactory.Framework.Logging;
using Microsoft.AspNetCore.Http;

namespace AppFactory.Framework.Api.AspNetCore.Core;

public class EndpointRequestHandler<TRequest, TResponse> : IEndpointRequestHandler<TRequest, TResponse>
    where TRequest : class, new()
    where TResponse : class
{
    private readonly IRequestParser _requestParser;
    private readonly IFunctionProcessor<TRequest, TResponse> _processor;
    private readonly IEndpointResponseMapper<TResponse> _responseMapper;
    private readonly ILogger? _logger;

    public EndpointRequestHandler(
        IRequestParser requestParser,
        IFunctionProcessor<TRequest, TResponse> processor,
        IEndpointResponseMapper<TResponse> responseMapper,
        ILogger? logger = null)
    {
        _requestParser = requestParser;
        _processor = processor;
        _responseMapper = responseMapper;
        _logger = logger;
    }

    public async Task HandleAsync(HttpContext context)
    {
        var requestContext = new AspNetCoreRequestContext(context);
        var responseBuilder = new AspNetCoreResponseBuilder(context);

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

            _responseMapper.Map(result, responseBuilder, context);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error processing request: {Message}", ex.Message);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Internal server error", exception = ex });
        }
    }
}
