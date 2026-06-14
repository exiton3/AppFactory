using System.Reflection;
using System.Text.RegularExpressions;

namespace AppFactory.Framework.DependencyInjection.AssemblyScanning;

/// <summary>
/// Filter for selecting classes during assembly scanning
/// </summary>
internal class ClassFilter : IClassFilter
{
    private readonly List<Assembly> _assemblies;
    private readonly bool _publicOnly;
    private readonly List<Func<Type, bool>> _filters = new();

    public ClassFilter(List<Assembly> assemblies, bool publicOnly)
    {
        _assemblies = assemblies;
        _publicOnly = publicOnly;
    }

    public IClassFilter AssignableTo(Type type)
    {
        _filters.Add(t =>
        {
            // Handle generic type definitions (e.g., IFunctionProcessor<,>)
            if (type.IsGenericTypeDefinition)
            {
                return t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == type)
                    || (t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == type);
            }

            // Handle regular types
            return type.IsAssignableFrom(t);
        });
        return this;
    }

    public IClassFilter AssignableTo<T>()
    {
        return AssignableTo(typeof(T));
    }

    public IClassFilter ImplementingInterface<T>()
    {
        var interfaceType = typeof(T);
        if (!interfaceType.IsInterface)
        {
            throw new ArgumentException($"Type {interfaceType.Name} is not an interface.", nameof(T));
        }

        _filters.Add(t => t.GetInterfaces().Contains(interfaceType) ||
                         (interfaceType.IsGenericTypeDefinition &&
                          t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)));
        return this;
    }

    public IClassFilter InNamespace(string @namespace)
    {
        _filters.Add(t => t.Namespace != null && t.Namespace.Equals(@namespace, StringComparison.Ordinal));
        return this;
    }

    public IClassFilter InNamespaceOf<T>()
    {
        var @namespace = typeof(T).Namespace;
        if (@namespace == null)
        {
            throw new ArgumentException($"Type {typeof(T).Name} does not have a namespace.", nameof(T));
        }

        _filters.Add(t => t.Namespace != null && t.Namespace.StartsWith(@namespace, StringComparison.Ordinal));
        return this;
    }

    public IClassFilter WithName(string pattern)
    {
        var regex = new Regex(pattern, RegexOptions.Compiled);
        _filters.Add(t => regex.IsMatch(t.Name));
        return this;
    }

    public IClassFilter WithNameEndingWith(string suffix)
    {
        _filters.Add(t => t.Name.EndsWith(suffix, StringComparison.Ordinal));
        return this;
    }

    public List<Type> GetFilteredTypes()
    {
        var types = new List<Type>();

        foreach (var assembly in _assemblies)
        {
            IEnumerable<Type> assemblyTypes;
            try
            {
                assemblyTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                assemblyTypes = ex.Types.Where(t => t != null)!;
            }

            var filteredTypes = assemblyTypes
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => !_publicOnly || t.IsPublic || t.IsNestedPublic);

            foreach (var filter in _filters)
            {
                filteredTypes = filteredTypes.Where(filter);
            }

            types.AddRange(filteredTypes);
        }

        return types;
    }
}
