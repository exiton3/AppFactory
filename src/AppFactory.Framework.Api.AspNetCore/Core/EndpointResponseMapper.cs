using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Api.Responses;
using AppFactory.Framework.Domain.ServiceResult;
using AppFactory.Framework.Shared.Serialization;

namespace AppFactory.Framework.Api.AspNetCore.Core;

public class EndpointResponseMapper<TResponse> : IEndpointResponseMapper<TResponse>
    where TResponse : class
{
    private readonly IJsonSerializer _jsonSerializer;

    public EndpointResponseMapper(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public HttpResponse Map(Result<TResponse> result)
    {
        var builder = new HttpResponseBuilder(_jsonSerializer);

        switch (result.ResultType)
        {
            case ResultType.Ok:
                return builder
                    .StatusCode(HttpStatusCode.OK)
                    .Body(_jsonSerializer.Serialize(result.Data))
                    .Build();

            case ResultType.Accepted:
                return builder
                    .StatusCode(HttpStatusCode.Accepted)
                    .Body(_jsonSerializer.Serialize(result.Data))
                    .Build();

            case ResultType.Invalid:
                return builder
                    .StatusCode(HttpStatusCode.BadRequest)
                    .ErrorType("ValidationException")
                    .Errors(result.Errors)
                    .Body(new ProblemResponse
                    {
                        Problem = "Validation failed",
                        Errors = result.Errors.ToList()
                    })
                    .Build();

            case ResultType.NotFound:
                return builder
                    .StatusCode(HttpStatusCode.NotFound)
                    .ErrorType("NotFoundException")
                    .Body(new ProblemResponse
                    {
                        Problem = "Resource not found",
                        Errors = result.Errors.ToList()
                    })
                    .Build();

            case ResultType.Unauthorized:
                return builder
                    .StatusCode(HttpStatusCode.Unauthorized)
                    .ErrorType("UnauthorizedException")
                    .Body(new ProblemResponse
                    {
                        Problem = "Unauthorized",
                        Errors = result.Errors.ToList()
                    })
                    .Build();

            case ResultType.External:
                return builder
                    .StatusCode(HttpStatusCode.ServiceUnavailable)
                    .ErrorType("ExternalSystemError")
                    .Errors(result.Errors)
                    .Body(new ProblemResponse
                    {
                        Problem = "External system error",
                        Errors = result.Errors.ToList()
                    })
                    .Build();

            case ResultType.Unexpected:
                return builder
                    .StatusCode(HttpStatusCode.InternalServerError)
                    .ErrorType("InternalServerError")
                    .Errors(result.Errors)
                    .Body(new ProblemResponse
                    {
                        Problem = "Unexpected error",
                        Errors = result.Errors.ToList()
                    })
                    .Build();

            default:
                throw new ArgumentOutOfRangeException(nameof(result.ResultType), result.ResultType, "Unknown result type");
        }
    }
}
