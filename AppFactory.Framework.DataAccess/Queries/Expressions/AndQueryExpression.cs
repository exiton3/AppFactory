namespace AppFactory.Framework.DataAccess.Queries.Expressions;

class AndQueryExpression : IQueryExpression
{
    private readonly IQueryExpression _left;
    private readonly IQueryExpression _right;

    public AndQueryExpression(IQueryExpression left, IQueryExpression right)
    {
        _left = left;
        _right = right;
    }

    public string Evaluate()
    {
        return _left.Evaluate() + " and " + _right.Evaluate();
    }

    public IQueryExpression And(IQueryExpression keyExpression)
    {
        return new AndQueryExpression(this, keyExpression);
    }
}