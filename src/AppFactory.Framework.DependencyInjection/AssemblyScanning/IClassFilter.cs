namespace AppFactory.Framework.DependencyInjection.AssemblyScanning;

/// <summary>
/// Defines filters for selecting classes during assembly scanning
/// </summary>
public interface IClassFilter
{
    /// <summary>
    /// Filters classes that are assignable to the specified type
    /// </summary>
    /// <param name="type">The type to check assignability against</param>
    /// <returns>The class filter for chaining</returns>
    IClassFilter AssignableTo(Type type);

    /// <summary>
    /// Filters classes that are assignable to the specified generic type
    /// </summary>
    /// <typeparam name="T">The type to check assignability against</typeparam>
    /// <returns>The class filter for chaining</returns>
    IClassFilter AssignableTo<T>();

    /// <summary>
    /// Filters classes that implement the specified interface
    /// </summary>
    /// <typeparam name="T">The interface type</typeparam>
    /// <returns>The class filter for chaining</returns>
    IClassFilter ImplementingInterface<T>();

    /// <summary>
    /// Filters classes in the specified namespace
    /// </summary>
    /// <param name="namespace">The namespace to filter by</param>
    /// <returns>The class filter for chaining</returns>
    IClassFilter InNamespace(string @namespace);

    /// <summary>
    /// Filters classes in namespaces starting with the specified prefix
    /// </summary>
    /// <param name="namespacePrefix">The namespace prefix</param>
    /// <returns>The class filter for chaining</returns>
    IClassFilter InNamespaceOf<T>();

    /// <summary>
    /// Filters classes with names matching the specified pattern
    /// </summary>
    /// <param name="pattern">The pattern to match</param>
    /// <returns>The class filter for chaining</returns>
    IClassFilter WithName(string pattern);

    /// <summary>
    /// Filters classes with names ending with the specified suffix
    /// </summary>
    /// <param name="suffix">The suffix to match</param>
    /// <returns>The class filter for chaining</returns>
    IClassFilter WithNameEndingWith(string suffix);
}
