namespace AppFactory.Framework.DataAccess.Queries.Expressions;

public class BeginsWithCondition : QueryExpression
{

    public override string Evaluate()
    {
        return $@"begins_with({Name},{Value})";
    }

    public BeginsWithCondition(IQueryExpression name, IQueryExpression value) : base(name, value)
    {
    }
}