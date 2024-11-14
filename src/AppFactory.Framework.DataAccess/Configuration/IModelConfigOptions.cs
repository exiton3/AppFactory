namespace AppFactory.Framework.DataAccess.Configuration;

public interface IModelConfigOptions<TModel> where TModel : class
{
    IModelConfigOptions<TModel> PKPrefix(string prefix);
    IModelConfigOptions<TModel> SKPrefix(string prefix);
    IModelConfigOptions<TModel> PK<TKey>(Func<TModel, TKey> id);
    IModelConfigOptions<TModel> SK<TKey>(Func<TModel, TKey> id);
    IModelConfigOptions<TModel> Id<TKey>(Func<TModel, TKey> id);
}