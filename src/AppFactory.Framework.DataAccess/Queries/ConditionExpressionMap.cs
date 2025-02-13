using System.Linq.Expressions;
using AppFactory.Framework.DataAccess.Models;
using AppFactory.Framework.DataAccess.Queries.Expressions;
using AppFactory.Framework.Shared;

namespace AppFactory.Framework.DataAccess.Queries;

public abstract class ConditionExpressionMap<TModel>: QueryModelExpression<TModel> where TModel : ModelBase
{
    private readonly List<KeyConditionOptions> _conditions = new();
    protected ISKConditionOptions ConditionFor<TProp>(Expression<Func<TModel, TProp>> propertyExpression)
    {
        var propName = PropertyExpressionHelper.GetPropertyName(propertyExpression);

        var propKeyValueName = propName.ToLower() + "Value";

        var keyOptions = new KeyConditionOptions(propName, propKeyValueName);

        _conditions.Add(keyOptions);

        return keyOptions;
    }

    public override string Evaluate()
    {
       var expression = _conditions[0].QueryExpression;

       for (var i = 1; i < _conditions.Count; i++)
       {
           var condition = _conditions[i];

           expression = expression.And(condition.QueryExpression);
       }

       return expression.Evaluate();
    }
}

public class TestModel : ModelBase
{
    public string Name { get; set; }
}
public class ConditionExpressionMapImpl : ConditionExpressionMap<TestModel> 
{
    public ConditionExpressionMapImpl()
    {
        ConditionFor(x => x.Name).Equals("asdf");
    }
}