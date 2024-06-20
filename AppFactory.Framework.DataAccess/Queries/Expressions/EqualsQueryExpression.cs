namespace AppFactory.Framework.DataAccess.Queries.Expressions;

internal class EqualsQueryExpression : QueryExpression
{
    public EqualsQueryExpression(IQueryExpression name, IQueryExpression value) : base(name, value)
    {
    }
    
    public EqualsQueryExpression(string name, string value):base(new KeyName(name), new KeyValue(value))
    {
        
    }
    public override string Evaluate()
    {
        return $@"{Name} = {Value}";
    }
}