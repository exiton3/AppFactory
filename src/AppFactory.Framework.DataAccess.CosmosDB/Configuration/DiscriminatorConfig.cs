namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

/// <summary>
/// Describes how a class hierarchy is stored: the discriminator property written into each document
/// object and the value identifying each concrete type.
/// </summary>
internal sealed class DiscriminatorConfig
{
    private readonly Dictionary<string, Type> _typesByValue = new Dictionary<string, Type>(StringComparer.Ordinal);
    private readonly Dictionary<Type, string> _valuesByType = new Dictionary<Type, string>();

    public DiscriminatorConfig(Type baseType, string description)
    {
        BaseType = baseType;
        Description = description;
    }

    /// <summary>
    /// The type all registered types must be assignable to.
    /// </summary>
    public Type BaseType { get; }

    /// <summary>
    /// Where the configuration was declared, used in error messages (e.g. "Polymorphic&lt;Person&gt;()").
    /// </summary>
    public string Description { get; }

    public string? PropertyName { get; private set; }

    public Type? DefaultType { get; private set; }

    public IReadOnlyDictionary<Type, string> ValuesByType => _valuesByType;

    public void SetPropertyName(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Discriminator property name must be specified", nameof(propertyName));
        }

        if (PropertyName != null)
        {
            throw new InvalidOperationException($"{Description}: discriminator property is already set to '{PropertyName}'");
        }

        PropertyName = propertyName;
    }

    public void AddValue(Type type, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{Description}: discriminator value for {type.Name} must be specified", nameof(value));
        }

        EnsureConcreteAndAssignable(type);

        if (_typesByValue.TryGetValue(value, out var existingType))
        {
            throw new InvalidOperationException($"{Description}: discriminator value '{value}' is already used by {existingType.Name}");
        }

        if (_valuesByType.TryGetValue(type, out var existingValue))
        {
            throw new InvalidOperationException($"{Description}: {type.Name} is already registered with discriminator value '{existingValue}'");
        }

        _typesByValue.Add(value, type);
        _valuesByType.Add(type, value);
    }

    public void SetDefaultType(Type type)
    {
        EnsureConcreteAndAssignable(type);
        DefaultType = type;
    }

    public bool TryGetType(string value, out Type type) => _typesByValue.TryGetValue(value, out type!);

    /// <summary>
    /// Throws when the configuration is incomplete. Called once when the model mapper is created.
    /// </summary>
    public void Validate()
    {
        if (PropertyName == null)
        {
            throw new InvalidOperationException($"{Description}: discriminator property is not set. Call DiscriminateOn(\"...\").");
        }

        if (_valuesByType.Count == 0)
        {
            throw new InvalidOperationException($"{Description}: no types registered. Call WithValue<T>(\"...\").");
        }
    }

    private void EnsureConcreteAndAssignable(Type type)
    {
        if (type.IsAbstract || type.IsInterface)
        {
            throw new ArgumentException($"{Description}: {type.Name} is abstract and cannot be created when reading a document");
        }

        if (!BaseType.IsAssignableFrom(type))
        {
            throw new ArgumentException($"{Description}: {type.Name} is not assignable to {BaseType.Name}");
        }
    }
}
