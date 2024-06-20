namespace AppFactory.Framework.DataAccess.Queries.Expressions;

public abstract class QueryExpression : QueryExpressionBase
{
    protected QueryExpression(IQueryExpression name, IQueryExpression value)
    {
        Name = name;
        Value = value;
    }

    public IQueryExpression Name { get; set; }
    public IQueryExpression Value { get; set; }

    public override string ToString()
    {
        return Evaluate();
    }
}