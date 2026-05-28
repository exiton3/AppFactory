namespace AppFactory.Framework.Api.Abstractions;

/// <summary>
/// Platform-agnostic abstraction over HTTP request context
/// Supports AWS Lambda (API Gateway), Azure Functions, and ASP.NET Core
/// </summary>
public interface IHttpRequestContext
{
    /// <summary>
    /// Unique request identifier (e.g., AWS RequestId, Azure InvocationId, ASP.NET TraceId)
    /// </summary>
    string RequestId { get; }

    /// <summary>
    /// HTTP method (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    HttpMethod Method { get; }

    /// <summary>
    /// Path parameters extracted from route (e.g., /users/{userId})
    /// </summary>
    IDictionary<string, string> PathParameters { get; }

    /// <summary>
    /// Query string parameters
    /// </summary>
    IDictionary<string, string> QueryParameters { get; }

    /// <summary>
    /// HTTP headers
    /// </summary>
    IDictionary<string, string> Headers { get; }

    /// <summary>
    /// Request body as string
    /// </summary>
    string Body { get; }

    /// <summary>
    /// Request body as stream (for large payloads)
    /// </summary>
    Stream BodyStream { get; }

    /// <summary>
    /// Content-Type header value
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Request path (e.g., /api/users/123)
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Query string (e.g., ?page=1&size=10)
    /// </summary>
    string QueryString { get; }
}
