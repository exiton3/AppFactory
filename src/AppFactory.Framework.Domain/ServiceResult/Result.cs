namespace AppFactory.Framework.Domain.ServiceResult;

public abstract class Result<T>
{
    public abstract ResultType ResultType { get; }
    public abstract List<Error> Errors { get; }
    public abstract T Data { get; }
}