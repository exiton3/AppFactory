namespace AppFactory.Framework.DataAccess.Queries.Expressions;

public abstract class QueryExpressionBase : IQueryExpression
{

    public abstract string Evaluate();

    public IQueryExpression And(IQueryExpression keyExpression) => new AndQueryExpression(this, keyExpression);

    public override string ToString()
    {
        return Evaluate();
    }
}