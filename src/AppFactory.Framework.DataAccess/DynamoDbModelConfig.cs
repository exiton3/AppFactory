using AppFactory.Framework.DataAccess.Models;

namespace AppFactory.Framework.DataAccess;

public class DynamoDbModelConfig<TModel> where TModel : class
{
    protected string PKPattern => $"{PKPrefix}{DynamoDBConstants.Separator}{{0}}";
    protected string SKPattern => $"{SKPrefix}{DynamoDBConstants.Separator}{{0}}";
    public string PKPrefix { get; set; }
    public string SKPrefix { get; set; }
    public Func<TModel, object> Id { get; set; }

    public string GetPKValue(TModel model)
    {
        return GetPKValue(Id(model));
    }

    public string GetSKValue(TModel model)
    {
        return GetSKValue(Id(model));
    }

    public string GetPKValue(object key)
    {
        return string.IsNullOrEmpty(PKPrefix) ? key.ToString() : string.Format(PKPattern, key);
    }

    public string GetSKValue(object key)
    {
        return string.IsNullOrEmpty(SKPrefix) ? key.ToString() : string.Format(SKPattern, key);
    }

    public PrimaryKey GetPrimaryKey(TModel model)
    {
       return GetPrimaryKey(Id(model));
    }

    public PrimaryKey GetPrimaryKey(object key)
    {
        return new PrimaryKey
        {
            PK = GetPKValue(key),
            SK = GetSKValue(key)
        };
    }
}