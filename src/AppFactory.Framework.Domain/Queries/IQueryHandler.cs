namespace AppFactory.Framework.Domain.Queries;


public interface IQueryHandler<in TRequest, TResponse> where TRequest : IQueryRequest
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}