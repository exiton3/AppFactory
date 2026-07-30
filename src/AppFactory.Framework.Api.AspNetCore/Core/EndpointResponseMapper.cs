using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Api.Responses;
using AppFactory.Framework.Domain.ServiceResult;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.AspNetCore.Http;

namespace AppFactory.Framework.Api.AspNetCore.Core;

public class EndpointResponseMapper<TResponse> : IEndpointResponseMapper<TResponse>
    where TResponse : class
{
    private readonly IJsonSerializer _jsonSerializer;

    public EndpointResponseMapper(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public void Map(Result<TResponse> result, AspNetCoreResponseBuilder responseBuilder, HttpContext context)
    {
        switch (result.ResultType)
        {
            case ResultType.Ok:
                responseBuilder
                    .StatusCode(HttpStatusCode.OK)
                    .Body(_jsonSerializer.Serialize(result.Data));
                break;

            case ResultType.Accepted:
                responseBuilder
                    .StatusCode(HttpStatusCode.Accepted)
                    .Body(_jsonSerializer.Serialize(result.Data));
                break;

            case ResultType.Invalid:
                responseBuilder
                    .StatusCode(HttpStatusCode.BadRequest)
                    .ErrorType("ValidationException")
                    .Errors(result.Errors)
                    .Body(new ProblemResponse
                    {
                        Problem = "Validation failed",
                        Errors = result.Errors.ToList()
                    });
                break;

            case ResultType.NotFound:
                responseBuilder
                    .StatusCode(HttpStatusCode.NotFound)
                    .ErrorType("NotFoundException")
                    .Body(new { message = string.Join(", ", result.Errors.Select(e => e.Message)) });
                break;

            case ResultType.Unauthorized:
                responseBuilder
                    .StatusCode(HttpStatusCode.Unauthorized)
                    .ErrorType("UnauthorizedException")
                    .Body(new { message = "Unauthorized" });
                break;

            case ResultType.External:
                responseBuilder
                    .StatusCode(HttpStatusCode.ServiceUnavailable)
                    .ErrorType("ExternalSystemError")
                    .Errors(result.Errors)
                    .Body(new ProblemResponse
                    {
                        Problem = "External system error",
                        Errors = result.Errors.ToList()
                    });
                break;

            case ResultType.Unexpected:
                responseBuilder
                    .StatusCode(HttpStatusCode.InternalServerError)
                    .ErrorType("InternalServerError")
                    .Errors(result.Errors)
                    .Body(new ProblemResponse
                    {
                        Problem = "Unexpected error",
                        Errors = result.Errors.ToList()
                    });
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(result.ResultType), result.ResultType, "Unknown result type");
        }

        responseBuilder.Build();
    }
}
