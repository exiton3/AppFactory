namespace AppFactory.Framework.Domain.ServiceResult;

public class NotFoundResult<T> : Result<T>
{
    private readonly Error _error;
    public NotFoundResult(string error)
    {
        _error = new Error { Code = "104", Message = error };
    }

    public NotFoundResult(Error error)
    {
        _error = error;
    }
    public NotFoundResult(string code, string error)
    {
        _error = new Error { Code = code, Message = error };
    }
    public override ResultType ResultType => ResultType.NotFound;

    public override List<Error> Errors => new List<Error> { _error ?? new Error() };

    public override T Data => default;
}