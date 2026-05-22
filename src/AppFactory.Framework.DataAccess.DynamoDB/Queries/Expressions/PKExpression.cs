using AppFactory.Framework.DataAccess.DynamoDB.Configuration;

namespace AppFactory.Framework.DataAccess.DynamoDB.Queries.Expressions;

public class PKExpression : QueryExpressionBase
{
    private readonly string _pkName;

    public PKExpression(string pk = null)
    {
        _pkName = pk?? DynamoDbConstants.PK;
    }

    public string PKValue => _pkName.ToLower() + "Value";


    public override string Evaluate()
    {
        return new EqualsQueryExpression(new KeyName(_pkName), new KeyValue(PKValue)).Evaluate();
    }
}