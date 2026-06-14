namespace AppFactory.Framework.Domain.ServiceResult;

public abstract class Result<T>
{
    public abstract ResultType ResultType { get; }
    public abstract List<Error> Errors { get; }
    public abstract T Data { get; }

    public static Result<T> Ok(T data) => new SuccessResult<T>(data);
    public static Result<T> Invalid(List<Error> errors) => new InvalidResult<T>(errors);
    public static Result<T> NotFound(string message = "Not found") => new NotFoundResult<T>(message);
}
