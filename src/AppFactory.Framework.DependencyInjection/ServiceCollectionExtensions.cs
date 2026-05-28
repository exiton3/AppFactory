using System.Reflection;
using AppFactory.Framework.DependencyInjection.AssemblyScanning;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.DependencyInjection;

/// <summary>
/// Extension methods for scanning assemblies and registering types
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Scans assemblies for types to register in the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="action">Action to configure assembly scanning</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection Scan(this IServiceCollection services, Action<IAssemblyScanningBuilder> action)
    {
        var builder = new AssemblyScanningBuilder(services);
        var scanner = builder.Build(action);
        scanner.Populate();
        return services;
    }
}

/// <summary>
/// Builder for configuring which assemblies to scan
/// </summary>
public interface IAssemblyScanningBuilder
{
    /// <summary>
    /// Scans assemblies containing the specified types
    /// </summary>
    /// <param name="types">Types whose assemblies should be scanned</param>
    /// <returns>The assembly scanner for further configuration</returns>
    IAssemblyScanner FromAssembliesOf(params Type[] types);

    /// <summary>
    /// Scans the specified assemblies
    /// </summary>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <returns>The assembly scanner for further configuration</returns>
    IAssemblyScanner FromAssemblies(params Assembly[] assemblies);

    /// <summary>
    /// Scans the calling assembly
    /// </summary>
    /// <returns>The assembly scanner for further configuration</returns>
    IAssemblyScanner FromCallingAssembly();

    /// <summary>
    /// Scans the entry assembly
    /// </summary>
    /// <returns>The assembly scanner for further configuration</returns>
    IAssemblyScanner FromEntryAssembly();

    /// <summary>
    /// Scans the assembly containing the specified type
    /// </summary>
    /// <typeparam name="T">Type whose assembly should be scanned</typeparam>
    /// <returns>The assembly scanner for further configuration</returns>
    IAssemblyScanner FromAssemblyOf<T>();
}

/// <summary>
/// Implementation of the assembly scanning builder
/// </summary>
internal class AssemblyScanningBuilder : IAssemblyScanningBuilder
{
    private readonly IServiceCollection _services;
    private AssemblyScanner? _currentScanner;

    public AssemblyScanningBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public IAssemblyScanner FromAssembliesOf(params Type[] types)
    {
        var assemblies = types.Select(t => t.Assembly).Distinct();
        return CreateScanner(assemblies);
    }

    public IAssemblyScanner FromAssemblies(params Assembly[] assemblies)
    {
        return CreateScanner(assemblies);
    }

    public IAssemblyScanner FromCallingAssembly()
    {
        return CreateScanner(new[] { Assembly.GetCallingAssembly() });
    }

    public IAssemblyScanner FromEntryAssembly()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
        {
            throw new InvalidOperationException("Entry assembly could not be determined.");
        }
        return CreateScanner(new[] { entryAssembly });
    }

    public IAssemblyScanner FromAssemblyOf<T>()
    {
        return CreateScanner(new[] { typeof(T).Assembly });
    }

    internal AssemblyScanner Build(Action<IAssemblyScanningBuilder> action)
    {
        action(this);
        if (_currentScanner == null)
        {
            throw new InvalidOperationException("No assemblies were specified for scanning.");
        }
        return _currentScanner;
    }

    private IAssemblyScanner CreateScanner(IEnumerable<Assembly> assemblies)
    {
        _currentScanner = new AssemblyScanner(_services, assemblies);
        return _currentScanner;
    }
}
