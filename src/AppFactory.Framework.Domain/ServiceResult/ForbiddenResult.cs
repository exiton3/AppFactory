namespace AppFactory.Framework.Domain.ServiceResult;

public class ForbiddenResult<T> : Result<T>
{
    private readonly Error _error;

    public ForbiddenResult(string error)
    {
        _error = new Error { Code = "103", Message = error };
    }

    public ForbiddenResult(Error error)
    {
        _error = error;
    }

    public ForbiddenResult(string code, string error)
    {
        _error = new Error { Code = code, Message = error };
    }

    public override ResultType ResultType => ResultType.Forbidden;

    public override List<Error> Errors => new List<Error> { _error ?? new Error() };

    public override T Data => default;
}
