namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

internal static class TypeHelper
{
    /// <summary>
    /// Returns the item type of an array or collection, the value type of a dictionary,
    /// or null when <paramref name="type"/> is not a collection (strings are not collections).
    /// </summary>
    public static Type? GetCollectionItemType(Type type)
    {
        if (type == typeof(string))
        {
            return null;
        }

        if (type.IsArray)
        {
            return type.GetElementType();
        }

        var dictionary = FindGenericInterface(type, typeof(IDictionary<,>)) ?? FindGenericInterface(type, typeof(IReadOnlyDictionary<,>));
        if (dictionary != null)
        {
            return dictionary.GetGenericArguments()[1];
        }

        return FindGenericInterface(type, typeof(IEnumerable<>))?.GetGenericArguments()[0];
    }

    /// <summary>
    /// Number of base classes between <paramref name="type"/> and object. Used to pick the most specific match.
    /// </summary>
    public static int GetInheritanceDepth(Type type)
    {
        var depth = 0;
        for (var current = type.BaseType; current != null; current = current.BaseType)
        {
            depth++;
        }

        return depth;
    }

    private static Type? FindGenericInterface(Type type, Type genericInterface)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == genericInterface)
        {
            return type;
        }

        return type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterface);
    }
}
