namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

public interface IModelConfig<TModel> where TModel : class
{
    void Configure(IModelConfigOptions<TModel> config);
}
