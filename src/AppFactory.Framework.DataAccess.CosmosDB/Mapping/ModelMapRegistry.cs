using System.Collections.Concurrent;
using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.DataAccess.CosmosDB.Mapping;

/// <summary>
/// Builds the mapping of each model once and shares it for the lifetime of the application.
/// </summary>
public interface IModelMapRegistry
{
    /// <summary>
    /// Returns the mapping of <typeparamref name="TModel"/>, building it from the registered
    /// <see cref="IModelConfig{TModel}"/> on first use.
    /// </summary>
    CosmosDbModelMap<TModel> Get<TModel>() where TModel : class;
}

// Singleton. Lazy guarantees Configure() runs exactly once per model, even when the first
// requests arrive concurrently (ConcurrentDictionary.GetOrAdd may call its factory more than once).
internal sealed class ModelMapRegistry : IModelMapRegistry
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, Lazy<object>> _maps = new ConcurrentDictionary<Type, Lazy<object>>();

    public ModelMapRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public CosmosDbModelMap<TModel> Get<TModel>() where TModel : class
    {
        var map = _maps.GetOrAdd(typeof(TModel), _ => new Lazy<object>(
            () => CosmosDbModelMap<TModel>.Create(_serviceProvider.GetRequiredService<IModelConfig<TModel>>())));

        return (CosmosDbModelMap<TModel>)map.Value;
    }
}
