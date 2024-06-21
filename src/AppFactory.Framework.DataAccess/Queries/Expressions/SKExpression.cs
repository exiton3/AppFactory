namespace AppFactory.Framework.DataAccess.Queries.Expressions;

public class SKExpression : QueryExpressionBase
{
    public override string Evaluate()
    {
        return new KeyName("SK").Evaluate();
    }
}