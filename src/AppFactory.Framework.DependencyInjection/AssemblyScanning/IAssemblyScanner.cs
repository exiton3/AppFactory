using System.Reflection;

namespace AppFactory.Framework.DependencyInjection.AssemblyScanning;

/// <summary>
/// Defines the contract for assembly scanning operations
/// </summary>
public interface IAssemblyScanner
{
    /// <summary>
    /// Adds classes matching the specified criteria
    /// </summary>
    /// <param name="filter">Filter to apply to classes</param>
    /// <param name="publicOnly">Whether to include only public types</param>
    /// <returns>The type selector for further configuration</returns>
    ITypeSelector AddClasses(Action<IClassFilter> filter, bool publicOnly = true);
}
