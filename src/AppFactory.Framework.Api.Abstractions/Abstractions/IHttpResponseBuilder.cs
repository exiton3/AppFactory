using AppFactory.Framework.Domain.ServiceResult;

namespace AppFactory.Framework.Api.Abstractions;

/// <summary>
/// Platform-agnostic HTTP response builder
/// Provides fluent API for building responses across AWS Lambda, Azure Functions, and ASP.NET Core
/// </summary>
public interface IHttpResponseBuilder
{
    /// <summary>
    /// Set HTTP status code
    /// </summary>
    IHttpResponseBuilder StatusCode(int statusCode);

    /// <summary>
    /// Add HTTP header
    /// </summary>
    IHttpResponseBuilder Header(string key, string value);

    /// <summary>
    /// Add multiple HTTP headers
    /// </summary>
    IHttpResponseBuilder Headers(IDictionary<string, string> headers);

    /// <summary>
    /// Set response body as string
    /// </summary>
    IHttpResponseBuilder Body(string body);

    /// <summary>
    /// Set response body as serialized object
    /// </summary>
    IHttpResponseBuilder Body<T>(T data);

    /// <summary>
    /// Set Content-Type header
    /// </summary>
    IHttpResponseBuilder ContentType(string contentType);

    /// <summary>
    /// Set error type (e.g., ValidationException, NotFoundException)
    /// </summary>
    IHttpResponseBuilder ErrorType(string errorType);

    /// <summary>
    /// Set problem details title
    /// </summary>
    IHttpResponseBuilder ProblemTitle(string title);

    /// <summary>
    /// Set errors in problem details format
    /// </summary>
    IHttpResponseBuilder Errors(IEnumerable<Error> errors);

    /// <summary>
    /// Build platform-specific response object
    /// For AWS Lambda: APIGatewayProxyResponse
    /// For Azure Functions: HttpResponseData
    /// For ASP.NET Core: IResult or sets HttpContext.Response
    /// </summary>
    object Build();
}
