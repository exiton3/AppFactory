using AppFactory.Framework.DataAccess.CosmosDB.Configuration;

namespace AppFactory.Framework.DataAccess.CosmosDB.Mapping;

/// <summary>
/// The configured mapping of a model: the result of running its <see cref="IModelConfig{TModel}"/> once.
/// Shared by all repositories and requests, so it is never modified after it is built.
/// </summary>
public sealed class CosmosDbModelMap<TModel> where TModel : class
{
    internal CosmosDbModelMap(CosmosDbModelConfig<TModel> config, IModelMapper<TModel> mapper)
    {
        Config = config;
        Mapper = mapper;
    }

    internal CosmosDbModelConfig<TModel> Config { get; }

    internal IModelMapper<TModel> Mapper { get; }

    internal static CosmosDbModelMap<TModel> Create(IModelConfig<TModel> modelConfig)
    {
        var config = new CosmosDbModelConfig<TModel>();
        modelConfig.Configure(config);

        return new CosmosDbModelMap<TModel>(config, new ModelMapper<TModel>(config));
    }
}
