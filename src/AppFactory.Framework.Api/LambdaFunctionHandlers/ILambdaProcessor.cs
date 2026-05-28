using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Domain.ServiceResult;

namespace AppFactory.Framework.Api.LambdaFunctionHandlers;

/// <summary>
/// AWS Lambda-specific processor interface (deprecated)
/// Use IFunctionProcessor from AppFactory.Framework.Api.Abstractions for platform-agnostic code
/// </summary>
[Obsolete("Use IFunctionProcessor<TRequest, TResponse> from AppFactory.Framework.Api.Abstractions namespace for platform-agnostic code. This interface is kept for backward compatibility.")]
public interface ILambdaProcessor<TRequest, TResponse> : IFunctionProcessor<TRequest, TResponse> 
    where TRequest : class, new() 
    where TResponse : class
{
}