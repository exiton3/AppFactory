namespace AppFactory.Framework.Domain.ServiceResult;

public class ExternalErrorResult<T> : Result<T>
{
    private readonly List<Error> _errors = new();
   

    public ExternalErrorResult(Error error)
    {
        _errors = new List<Error> { error };
    }

    public ExternalErrorResult(IEnumerable<Error> errors)
    {
        _errors.AddRange(errors);
    }
    public ExternalErrorResult(string code, string error):this( new Error { Code = code, Message = error})
    {
        
    }
    public override ResultType ResultType => ResultType.External;

    public override List<Error> Errors => _errors;

    public override T Data => default;
}