namespace AppFactory.Framework.Domain.ServiceResult;

public class AcceptedResult<T> : Result<T>
{
    private readonly T _data;
    public AcceptedResult(T data)
    {
        _data = data;
    }
    public override ResultType ResultType => ResultType.Accepted;

    public override List<Error> Errors => new();

    public override T Data => _data;
}