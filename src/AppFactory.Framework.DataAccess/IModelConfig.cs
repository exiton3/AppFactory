namespace AppFactory.Framework.DataAccess;

public interface IModelConfig<TModel> where TModel : class
{
    void Configure(IModelConfigOptions<TModel> config);
}