namespace AppFactory.Framework.Domain.ServiceResult;

public class UnauthorizedResult<T> : Result<T>
{
    private readonly Error _error;

    public UnauthorizedResult(string error)
    {
        _error = new Error { Code = "101", Message = error };
    }

    public UnauthorizedResult(Error error)
    {
        _error = error;
    }

    public UnauthorizedResult(string code, string error)
    {
        _error = new Error { Code = code, Message = error };
    }

    public override ResultType ResultType => ResultType.Unauthorized;

    public override List<Error> Errors => new List<Error> { _error ?? new Error() };

    public override T Data => default;
}
