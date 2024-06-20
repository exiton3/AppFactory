using AppFactory.Framework.Domain.ServiceResult;

namespace AppFactory.Framework.Api.LambdaFunctionHandlers;

public interface ILambdaProcessor<TRequest, TResponse> where TRequest : class, new() where TResponse : class
{
    Task<Result<TResponse>> Process(TRequest request, CancellationToken cancellationToken = default);
}