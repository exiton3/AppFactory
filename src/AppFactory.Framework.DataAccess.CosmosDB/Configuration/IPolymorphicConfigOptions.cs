namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

/// <summary>
/// Fluent configuration of a polymorphic type or property
/// </summary>
/// <typeparam name="TBase">The type all registered types must derive from</typeparam>
public interface IPolymorphicConfigOptions<TBase> where TBase : class
{
    /// <summary>
    /// Sets the name of the property written into each stored object to identify its concrete type
    /// (e.g. "type"). The name is stored as given. It must not clash with a property of the registered types.
    /// </summary>
    IDiscriminatorConfigOptions<TBase> DiscriminateOn(string propertyName);
}

/// <summary>
/// Fluent registration of the concrete types of a polymorphic type or property
/// </summary>
/// <typeparam name="TBase">The type all registered types must derive from</typeparam>
public interface IDiscriminatorConfigOptions<TBase> where TBase : class
{
    /// <summary>
    /// Registers a concrete type and the discriminator value stored for it.
    /// A concrete base type needs its own value too.
    /// </summary>
    IDiscriminatorConfigOptions<TBase> WithValue<TDerived>(string value) where TDerived : class, TBase;

    /// <summary>
    /// The type used when a stored object has no discriminator, e.g. documents written before it was configured.
    /// Without it, reading such an object fails unless the declared property type is concrete.
    /// </summary>
    IDiscriminatorConfigOptions<TBase> DefaultTo<TDerived>() where TDerived : class, TBase;
}
