using System.Linq.Expressions;

namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

public interface IModelConfigOptions<TModel> where TModel : class
{
    /// <summary>
    /// Sets the Cosmos DB container name
    /// </summary>
    IModelConfigOptions<TModel> ContainerName(string containerName);

    /// <summary>
    /// Sets the ID prefix
    /// </summary>
    IModelConfigOptions<TModel> IdPrefix(string prefix);

    /// <summary>
    /// Sets the partition key selector and returns a fluent interface for configuring this partition key part
    /// Allows chaining .WithName() and .WithPrefix()
    /// </summary>
    IPartitionKeyConfigOptions<TModel> PartitionKey<TKey>(Expression<Func<TModel, TKey>> propertyExpression);

    /// </summary>
    IPartitionKeyConfigOptions<TModel> PartitionKey(string propertyName);

    /// <summary>
    /// Sets the ID selector
    /// </summary>
    IModelConfigOptions<TModel> Id<TKey>(Expression<Func<TModel, TKey>> propertyExpression);

    /// <summary>
    /// Sets the Time-To-Live in seconds
    /// </summary>
    IModelConfigOptions<TModel> TimeToLive(int? ttlInSeconds);

    /// <summary>
    /// Stores and restores a class hierarchy wherever <typeparamref name="TBase"/> (or a derived type) is used
    /// in the model: single properties, collections and dictionary values, at any depth.
    /// <code>
    /// config.Polymorphic&lt;Person&gt;()
    ///       .DiscriminateOn("type")
    ///       .WithValue&lt;Person&gt;("person")
    ///       .WithValue&lt;Manager&gt;("manager");
    /// </code>
    /// </summary>
    IPolymorphicConfigOptions<TBase> Polymorphic<TBase>() where TBase : class;

    /// <summary>
    /// Configures a single property of the model. Polymorphism configured here overrides
    /// <see cref="Polymorphic{TBase}"/> for this property only.
    /// <code>
    /// config.Map(d => d.Persons)
    ///       .DiscriminateOn("personType")
    ///       .WithValue&lt;Person&gt;("person")
    ///       .WithValue&lt;Manager&gt;("manager");
    /// </code>
    /// </summary>
    IPolymorphicConfigOptions<object> Map<TProperty>(Expression<Func<TModel, TProperty>> propertyExpression);
}

