using AppFactory.Framework.DataAccess.Configuration;

namespace AppFactory.Framework.DataAccess.Queries.Expressions;

public class PKExpression : QueryExpressionBase
{
    private readonly string _pkName;

    public PKExpression(string pk = null)
    {
        _pkName = pk?? DynamoDBConstants.PK;
    }

    public string PKValue => _pkName.ToLower() + "Value";


    public override string Evaluate()
    {
        return new EqualsQueryExpression(new KeyName(_pkName), new KeyValue(PKValue)).Evaluate();
    }
}