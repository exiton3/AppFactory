using AppFactory.Framework.DataAccess.Models;

namespace AppFactory.Framework.DataAccess;

public class DynamoDbModelConfig<TModel> : IModelConfigOptions<TModel> where TModel : class
{
    protected string PKPattern => $"{_pkPrefix}{DynamoDBConstants.Separator}{{0}}";
    protected string SKPattern => $"{_skPrefix}{DynamoDBConstants.Separator}{{0}}";
    private string _pkPrefix;
    private string _skPrefix;
    private Func<TModel, object> _id;

    internal string GetPKValue(TModel model)
    {
        return GetPKValue(_id(model));
    }

    internal string GetSKValue(TModel model)
    {
        return GetSKValue(_id(model));
    }

    internal string GetPKValue(object key)
    {
        return string.IsNullOrEmpty(_pkPrefix) ? key.ToString() : string.Format(PKPattern, key);
    }

    internal string GetSKValue(object key)
    {
        return string.IsNullOrEmpty(_skPrefix) ? key.ToString() : string.Format(SKPattern, key);
    }

    public PrimaryKey GetPrimaryKey(TModel model)
    {
       return GetPrimaryKey(_id(model));
    }

    public PrimaryKey GetPrimaryKey(object key)
    {
        return new PrimaryKey
        {
            PK = GetPKValue(key),
            SK = GetSKValue(key)
        };
    }

    public IModelConfigOptions<TModel> PKPrefix(string prefix)
    {
        _pkPrefix = prefix;

        return this;
    }

    public IModelConfigOptions<TModel> SKPrefix(string prefix)
    {
        _skPrefix = prefix;

        return this;
    }

    public IModelConfigOptions<TModel> Id<TKey>(Func<TModel, TKey> id)
    {
        _id = o => id(o);

        return this;
    }
}