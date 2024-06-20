namespace AppFactory.Framework.Domain.ServiceResult;

public class InvalidResult<T> : Result<T>
{
    private readonly List<Error> _errors = new();
  

    public InvalidResult(IEnumerable<Error> errors)
    {
        _errors.AddRange(errors);
    }
    public override ResultType ResultType => ResultType.Invalid;

    public override List<Error> Errors =>   _errors ; 

    public override T Data => default;
}