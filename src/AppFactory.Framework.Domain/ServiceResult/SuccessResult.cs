namespace AppFactory.Framework.Domain.ServiceResult;

public class SuccessResult<T> : Result<T>
{
    private readonly T _data;
    public SuccessResult(T data)
    {
        _data = data;
    }
    public override ResultType ResultType => ResultType.Ok;

    public override List<Error> Errors => new();

    public override T Data => _data;
}