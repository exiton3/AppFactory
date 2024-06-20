namespace AppFactory.Framework.DataAccess.Queries.Expressions;

public class KeyValue : QueryExpressionBase
{
    private readonly string _keyValue;

    public KeyValue(string keyValue)
    {
        _keyValue = keyValue;
    }
    public override string Evaluate()
    {
        return $":{_keyValue}";
    }
}