namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

/// <summary>
/// Configuration for hierarchical partition keys (up to 3 levels)
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
public class PartitionKeyConfig<TModel> where TModel : class
{
    /// <summary>
    /// Collection of partition key parts (up to 3)
    /// </summary>
    public List<PartitionKeyPart<TModel>> Parts { get; set; } = new List<PartitionKeyPart<TModel>>();

    /// <summary>
    /// Indicates whether this configuration uses hierarchical partition keys
    /// </summary>
    public bool IsHierarchical => Parts.Any();

    /// <summary>
    /// Gets the number of partition key levels configured
    /// </summary>
    public int Count => Parts.Count;

    /// <summary>
    /// Gets the partition key paths derived from property names
    /// </summary>
    public List<string> GetPartitionKeyPaths()
    {
        return Parts
            .Select(p => p.DestinationPropertyName.StartsWith("/") ? p.DestinationPropertyName : $"/{p.DestinationPropertyName}")
            .ToList();
    }

    /// <summary>
    /// Adds a partition key part to the configuration
    /// </summary>
    /// <param name="part">The partition key part to add</param>
    /// <exception cref="InvalidOperationException">Thrown when trying to add more than 3 partition key parts</exception>
    public void AddPart(PartitionKeyPart<TModel> part)
    {
        if (Parts.Count >= 3)
            throw new InvalidOperationException("Cosmos DB supports up to 3 hierarchical partition keys");

        if (string.IsNullOrWhiteSpace(part.DestinationPropertyName))
            throw new ArgumentException("DestinationPropertyName must be specified for each partition key part", nameof(part));

        Parts.Add(part);
    }

    /// <summary>
    /// Gets hierarchical partition key values from the model
    /// </summary>
    /// <param name="model">The model to extract values from</param>
    /// <returns>List of partition key values with prefixes applied</returns>
    public List<string> GetValues(TModel model)
    {
        if (!IsHierarchical)
            return new List<string>();

        var values = new List<string>();
        foreach (var part in Parts)
        {
            object value;
            if (part.IsResolverSet)
            {
                value = part.Resolver.GetValue();
            }
            else
            {
                value = part.Getter(model);
            }
            var stringValue = value?.ToString() ?? string.Empty;

            // Apply prefix if configured
            if (!string.IsNullOrEmpty(part.Prefix))
            {
                stringValue = $"{part.Prefix}{CosmosDbConstants.Separator}{stringValue}";
            }

            values.Add(stringValue);
        }
        return values;
    }

   
    public Dictionary<string, string> GetPropertiesWithValues(TModel model)
    {
        if (!IsHierarchical)
            return new Dictionary<string, string>();

        var properties = new Dictionary<string, string>();
        foreach (var part in Parts)
        {
            var value = part.Getter(model);
            var stringValue = value?.ToString() ?? string.Empty;

            // Apply prefix if configured
            if (!string.IsNullOrEmpty(part.Prefix))
            {
                stringValue = $"{part.Prefix}{CosmosDbConstants.Separator}{stringValue}";
            }

            // Use the property name from the part
            var propertyName = part.DestinationPropertyName.TrimStart('/');
            properties[propertyName] = stringValue;
        }
        return properties;
    }
}
