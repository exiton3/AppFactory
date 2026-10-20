using System.Reflection;
using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AppFactory.Framework.DataAccess.CosmosDB.Mapping;

/// <summary>
/// Contract resolver of one model mapping: camelCase names, private setters and polymorphic properties.
/// </summary>
/// <remarks>
/// Derives from DefaultContractResolver, which caches contracts per instance. CamelCasePropertyNamesContractResolver
/// shares one static cache between all instances of the same resolver type, so per-model configuration
/// (discriminators) would leak between mappings. One instance is created per ModelMapper, which
/// <see cref="IModelMapRegistry"/> builds once per model.
/// </remarks>
internal sealed class CosmosDbContractResolver : DefaultContractResolver
{
    private readonly IReadOnlyList<DiscriminatorConfig> _typeDiscriminators;
    private readonly IReadOnlyDictionary<(Type DeclaringType, string Name), DiscriminatorConfig> _propertyDiscriminators;
    private readonly Dictionary<DiscriminatorConfig, DiscriminatorConverter> _converters;

    public CosmosDbContractResolver(
        IReadOnlyList<DiscriminatorConfig> typeDiscriminators,
        IReadOnlyDictionary<(Type DeclaringType, string Name), DiscriminatorConfig> propertyDiscriminators)
    {
        // Same naming as CamelCasePropertyNamesContractResolver
        NamingStrategy = new CamelCaseNamingStrategy
        {
            ProcessDictionaryKeys = true,
            OverrideSpecifiedNames = true
        };

        _typeDiscriminators = typeDiscriminators;
        _propertyDiscriminators = propertyDiscriminators;

        var all = typeDiscriminators.Concat(propertyDiscriminators.Values).ToList();
        all.ForEach(d => d.Validate());
        _converters = all.ToDictionary(d => d, d => new DiscriminatorConverter(d));

        all.ForEach(EnsureNoPropertyClash);
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        // Enables deserialization of properties with private setters — the standard DDD pattern
        // where aggregates expose state via public getters but restrict mutation to domain methods.
        if (!property.Writable && member is PropertyInfo prop && prop.GetSetMethod(nonPublic: true) != null)
        {
            property.Writable = true;
        }

        ApplyDiscriminator(member, property);

        return property;
    }

    private void ApplyDiscriminator(MemberInfo member, JsonProperty property)
    {
        if (property.PropertyType == null)
        {
            return;
        }

        var itemType = TypeHelper.GetCollectionItemType(property.PropertyType);
        var valueType = itemType ?? property.PropertyType;

        // Property-level configuration overrides type-level configuration
        var config = member.DeclaringType != null && _propertyDiscriminators.TryGetValue((member.DeclaringType, member.Name), out var propertyConfig)
            ? propertyConfig
            : FindTypeDiscriminator(valueType);

        if (config == null)
        {
            return;
        }

        if (itemType != null)
        {
            property.ItemConverter = _converters[config];
        }
        else
        {
            property.Converter = _converters[config];
        }
    }

    // The most specific registered base type wins, e.g. Polymorphic<Manager>() over Polymorphic<Person>() for a Manager property
    private DiscriminatorConfig? FindTypeDiscriminator(Type valueType)
    {
        return _typeDiscriminators
            .Where(d => d.BaseType.IsAssignableFrom(valueType))
            .OrderByDescending(d => TypeHelper.GetInheritanceDepth(d.BaseType))
            .FirstOrDefault();
    }

    // The discriminator is written next to the object's own properties, so it must not share a name with one of them
    private void EnsureNoPropertyClash(DiscriminatorConfig config)
    {
        foreach (var type in config.ValuesByType.Keys)
        {
            if (ResolveContract(type) is JsonObjectContract contract
                && contract.Properties.Any(p => string.Equals(p.PropertyName, config.PropertyName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    $"{config.Description}: discriminator '{config.PropertyName}' clashes with a property of {type.Name}. " +
                    "Use a different name in DiscriminateOn(\"...\").");
            }
        }
    }
}
