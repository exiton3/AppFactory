namespace AppFactory.Framework.Application.Queries;

/// <summary>
/// Handler for query requests that return data without modifying system state
/// </summary>
/// <typeparam name="TRequest">The query request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IQueryHandler<in TRequest, TResponse> where TRequest : IQueryRequest
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}
