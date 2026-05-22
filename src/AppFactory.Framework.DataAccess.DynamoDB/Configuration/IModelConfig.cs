namespace AppFactory.Framework.DataAccess.DynamoDB.Configuration;

public interface IModelConfig<TModel> where TModel : class
{
    void Configure(IModelConfigOptions<TModel> config);
}