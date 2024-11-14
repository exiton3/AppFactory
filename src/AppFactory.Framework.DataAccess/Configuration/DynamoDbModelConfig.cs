namespace AppFactory.Framework.DataAccess.Configuration;

public class DynamoDbModelConfig<TModel> : IModelConfigOptions<TModel> where TModel : class
{
    protected string PKPattern => $"{_pkPrefix}{DynamoDbConstants.Separator}{{0}}";
    protected string SKPattern => $"{_skPrefix}{DynamoDbConstants.Separator}{{0}}";
    private string _pkPrefix;
    private string _skPrefix;
    private Func<TModel, object> _pk;
    private Func<TModel, object> _sk;

    internal string GetPKValue(TModel model)
    {
        return GetPKValue(_pk(model));
    }

    internal string GetSKValue(TModel model)
    {
        return GetSKValue(_sk(model));
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
        return GetPrimaryKey(_pk(model), _sk(model));
    }

    public PrimaryKey GetPrimaryKey(object key)
    {
        return new PrimaryKey
        {
            PK = GetPKValue(key),
            SK = GetSKValue(key)
        };
    }

    public PrimaryKey GetPrimaryKey(object pk, object sk)
    {
        return new PrimaryKey
        {
            PK = GetPKValue(pk),
            SK = GetSKValue(sk)
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

    public IModelConfigOptions<TModel> PK<TKey>(Func<TModel, TKey> id)
    {
        _pk = o => id(o);

        return this;
    }

    public IModelConfigOptions<TModel> SK<TKey>(Func<TModel, TKey> id)
    {
        _sk = o => id(o);

        return this;
    }

    public IModelConfigOptions<TModel> Id<TKey>(Func<TModel, TKey> id)
    {
        _pk = o => id(o);
        _sk = o => id(o);

        return this;
    }
}