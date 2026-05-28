using AppFactory.Framework.Domain.ServiceResult;

namespace AppFactory.Framework.Api.Abstractions;

/// <summary>
/// Platform-agnostic function processor for CQRS commands and queries
/// Replaces ILambdaProcessor to work across AWS Lambda, Azure Functions, and ASP.NET Core
/// </summary>
/// <typeparam name="TRequest">Request model (Command or Query)</typeparam>
/// <typeparam name="TResponse">Response model (DTO)</typeparam>
public interface IFunctionProcessor<TRequest, TResponse> 
    where TRequest : class, new() 
    where TResponse : class
{
    /// <summary>
    /// Process the request and return a result
    /// </summary>
    /// <param name="request">Parsed and validated request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing response data or errors</returns>
    Task<Result<TResponse>> Process(TRequest request, CancellationToken cancellationToken = default);
}
