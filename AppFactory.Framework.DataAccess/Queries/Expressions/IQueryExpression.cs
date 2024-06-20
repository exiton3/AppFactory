namespace AppFactory.Framework.DataAccess.Queries.Expressions;

public interface IQueryExpression
{
    public string Evaluate();
    IQueryExpression And(IQueryExpression keyExpression);
}

public abstract class QueryModelExpression<TModel> : IQueryExpression
{
    public abstract string Evaluate();
    

    public IQueryExpression And(IQueryExpression keyExpression)
    {
       return new AndQueryExpression(this, keyExpression);
    }

    public override string ToString()
    {
       return Evaluate();
    }
}

class KeyName : QueryExpressionBase
{
    private readonly string _keyName;

    public KeyName(string keyName)
    {
        _keyName = keyName;
    }
    public override string Evaluate()
    {
        return _keyName;
    }
}

public static class ConditionsExtensions
{
    public static IQueryExpression PK(this IQueryExpression condition)
    {
        return new PKExpression();
    }

    public static IQueryExpression SK(this IQueryExpression condition)
    {
        return new SKExpression();
    }
}