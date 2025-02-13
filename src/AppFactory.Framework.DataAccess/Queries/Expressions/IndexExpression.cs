namespace AppFactory.Framework.DataAccess.Queries.Expressions;

public class IndexExpression : QueryExpressionBase
{
    private readonly string _indexName;

    public IndexExpression(string name)
    {
        _indexName = name;
    }

    public string Value => _indexName + "Value";


    public override string Evaluate()
    {
        return new EqualsQueryExpression(new KeyName(_indexName), new KeyValue(Value)).Evaluate();
    }
}