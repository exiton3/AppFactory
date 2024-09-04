namespace AppFactory.Framework.DataAccess;

public class DynamoDBModelConfigBuilder<TModel> where TModel : class
{
    private readonly DynamoDbModelConfig<TModel> _config;

    public DynamoDBModelConfigBuilder()
    {
        _config = new DynamoDbModelConfig<TModel>();
    }
    public DynamoDbModelConfig<TModel> Build()
    {
        return _config;
    }

    public DynamoDBModelConfigBuilder<TModel> PKPrefix(string prefix)
    {
        _config.PKPrefix = prefix;

        return this;
    }

    public DynamoDBModelConfigBuilder<TModel> SKPrefix(string prefix)
    {
        _config.SKPrefix = prefix;

        return this;
    }

    public DynamoDBModelConfigBuilder<TModel> Id<TKey>(Func<TModel,TKey> id)
    {
        _config.Id = o => id(o);

        return this;
    }
}