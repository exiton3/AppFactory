namespace AppFactory.Framework.DataAccess.Configuration;

public interface IModelConfig<TModel> where TModel : class
{
    void Configure(IModelConfigOptions<TModel> config);
}