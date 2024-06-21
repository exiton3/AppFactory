namespace AppFactory.Framework.Domain.ServiceResult;

public class UnexpectedResult<T> : Result<T>
{
    private readonly Exception _exception;

    private readonly Error _error;

    public UnexpectedResult( string error)
    {
        _error = new Error { Code = "100", Message = error };
    }
    public UnexpectedResult(Exception exception)
    {
        _exception = exception;
    }
    public override ResultType ResultType => ResultType.Unexpected;

    public override List<Error> Errors => new() { _error ?? new Error { Code = _exception.Message, Message = _exception.StackTrace }};

    public override T Data => default;
}